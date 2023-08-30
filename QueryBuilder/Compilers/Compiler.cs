using System.Diagnostics;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        protected WhiteList Operators { get; } = new();

        public string ParameterPrefix { get; protected init; } = "@p";
        public X XService { get; protected init; } = new("\"", "\"", "AS ");
        protected string TableAsKeyword { get; init; } = "AS ";
        protected string LastId { get; init; } = "";


        private const string SingleInsertStartClause = "INSERT INTO";
        protected string MultiInsertStartClause { get; init; } = "INSERT INTO";
        public string? EngineCode { get; protected init; }
        protected string? SingleRowDummyTableName { get; init; }

        /// <summary>
        ///     Whether the compiler supports the `SELECT ... FILTER` syntax
        /// </summary>
        /// <value></value>
        protected bool SupportsFilterClause { get; init; }

        /// <summary>
        ///     If true the compiler will remove the SELECT clause for the query used inside WHERE EXISTS
        /// </summary>
        /// <value></value>
        public bool OmitSelectInsideExists { get; init; } = true;

        /// <summary>
        ///     Add the passed operator(s) to the white list so they can be used with
        ///     the Where/Having methods, this prevent passing arbitrary operators
        ///     that opens the door for SQL injections.
        /// </summary>
        /// <param name="operators"></param>
        /// <returns></returns>
        public Compiler Whitelist(params string[] operators)
        {
            Operators.Whitelist(operators);

            return this;
        }

        private SqlResult CompileSelectQuery(Query query, Writer writer)
        {
            var ctx = new SqlResult();
            writer.Push(ctx);
            CompileSelectQueryInner(ctx, query, writer);

            return ctx;

        }

        public virtual void CompileSelectQueryInner(SqlResult ctx, Query query, Writer writer)
        {
            writer.WhitespaceSeparated(
                () => CompileColumns(ctx, query, writer),
                () => CompileFrom(ctx, query, writer),
                () => CompileJoins(ctx, query, writer),
                () => CompileWheres(ctx, query, writer),
                () => CompileGroups(ctx, query, writer),
                () => CompileHaving(ctx, query, writer),
                () => CompileOrders(ctx, query, writer),
                () => CompileLimit(ctx, query, writer),
                () => CompileUnion(ctx, query, writer));
            ctx.ReplaceRaw(writer);
        }

        protected virtual SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc, Writer writer)
        {
            var row = "SELECT " +
                      string.Join(", ", adHoc.Columns.Select(col => $"? AS {XService.Wrap(col)}"));

            var fromTable = SingleRowDummyTableName;

            if (fromTable != null) row += $" FROM {fromTable}";

            var rows = string.Join(" UNION ALL ", Enumerable.Repeat(row, adHoc.Values.Length / adHoc.Columns.Length));

            var ctx = new SqlResult(adHoc.Values, rows);
            writer.BindMany(adHoc.Values);
            writer.Push(ctx);
            return ctx;
        }

        public void CompileDeleteQuery(SqlResult ctx, Query query, Writer writer)
        {
            if (!query.HasComponent("from", EngineCode))
                throw new InvalidOperationException("No table set to delete");

            var fromClause = query.GetOneComponent<AbstractFrom>("from", EngineCode);

            string? table = null;

            if (fromClause is FromClause fromClauseCast) table = XService.Wrap(fromClauseCast.Table);

            if (fromClause is RawFromClause rawFromClause)
            {
                table = XService.WrapIdentifiers(rawFromClause.Expression);
                ctx.BindingsAddRange(rawFromClause.Bindings);
            }

            if (table is null) throw new InvalidOperationException("Invalid table expression");

            var joins = CompileJoins(ctx, query, writer.Sub());

            var where = CompileWheres(ctx, query, writer);

            if (!string.IsNullOrEmpty(where)) where = " " + where;

            if (string.IsNullOrEmpty(joins))
            {
                ctx.Raw.Append($"DELETE FROM {table}{where}");
            }
            else
            {
                // check if we have alias 
                if (fromClause is FromClause && !string.IsNullOrEmpty(fromClause.Alias))
                    ctx.Raw.Append($"DELETE {XService.Wrap(fromClause.Alias)} FROM {table} {joins}{where}");
                else
                    ctx.Raw.Append($"DELETE {table} FROM {table} {joins}{where}");
            }
        }

        public void CompileUpdateQuery(SqlResult ctx, Query query, Writer writer)
        {
            if (!query.HasComponent("from", EngineCode))
                throw new InvalidOperationException("No table set to update");

            var fromClause = query.GetOneComponent<AbstractFrom>("from", EngineCode);

            string? table = null;

            if (fromClause is FromClause fromClauseCast) table = XService.Wrap(fromClauseCast.Table);

            if (fromClause is RawFromClause rawFromClause)
            {
                table = XService.WrapIdentifiers(rawFromClause.Expression);
                ctx.BindingsAddRange(rawFromClause.Bindings);
            }

            if (table is null) throw new InvalidOperationException("Invalid table expression");

            // check for increment statements
            var clause = query.GetOneComponent("update", EngineCode);

            string? wheres;

            if (clause is IncrementClause increment)
            {
                var column = XService.Wrap(increment.Column);
                var value = Parameter(ctx, query, writer, Math.Abs(increment.Value));
                var sign = increment.Value >= 0 ? "+" : "-";

                wheres = CompileWheres(ctx, query, writer);

                if (!string.IsNullOrEmpty(wheres)) wheres = " " + wheres;

                ctx.Raw.Append($"UPDATE {table} SET {column} = {column} {sign} {value}{wheres}");

                return;
            }


            var toUpdate = query.GetOneComponent<InsertClause>("update", EngineCode);
            Debug.Assert(toUpdate != null);
            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Length; i++)
                parts.Add(XService.Wrap(toUpdate.Columns[i]) + " = " + Parameter(ctx, query, writer, toUpdate.Values[i]));

            var sets = string.Join(", ", parts);

            wheres = CompileWheres(ctx, query, writer);

            if (!string.IsNullOrEmpty(wheres)) wheres = " " + wheres;

            ctx.Raw.Append($"UPDATE {table} SET {sets}{wheres}");
        }

        public virtual void CompileInsertQuery(SqlResult ctx, Query query, Writer writer)
        {
            if (!query.HasComponent("from", EngineCode))
                throw new InvalidOperationException("No table set to insert");

            var fromClause = query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (fromClause is null)
                throw new InvalidOperationException("Invalid table expression");

            string? table = null;
            if (fromClause is FromClause fromClauseCast)
                table = XService.Wrap(fromClauseCast.Table);
            if (fromClause is RawFromClause rawFromClause)
            {
                table = XService.WrapIdentifiers(rawFromClause.Expression);
                ctx.BindingsAddRange(rawFromClause.Bindings);
            }

            if (table is null)
                throw new InvalidOperationException("Invalid table expression");
            writer.AssertMatches(ctx);

            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            if (inserts[0] is InsertQueryClause insertQueryClause)
            {
                writer.AssertMatches(ctx);
                CompileInsertQueryClause(insertQueryClause, writer);
                return;
            }
            writer.AssertMatches(ctx);
            CompileValueInsertClauses(inserts.Cast<InsertClause>().ToArray());
            return;


            void CompileInsertQueryClause(InsertQueryClause clause, Writer writer1)
            {
                var columns = clause.Columns.GetInsertColumnsList(XService);

                writer1.AssertMatches(ctx);
                var subCtx = CompileSelectQuery(clause.Query, writer1);
                ctx.BindingsAddRange(subCtx.Bindings);
                writer1.BindMany(subCtx.Bindings);
                writer1.Pop();
                writer1.AssertMatches(ctx);

                ctx.Raw.Append($"{SingleInsertStartClause} {table}{columns} {subCtx.RawSql}");
            }

            void CompileValueInsertClauses(InsertClause[] insertClauses)
            {
                var isMultiValueInsert = insertClauses.Length > 1;

                var insertInto = isMultiValueInsert ? MultiInsertStartClause : SingleInsertStartClause;

                var firstInsert = insertClauses.First();
                var columns = firstInsert.Columns.GetInsertColumnsList(XService);
                var values = string.Join(", ", Parametrize(ctx, writer, query, firstInsert.Values));

                ctx.Raw.Append($"{insertInto} {table}{columns} VALUES ({values})");

                if (isMultiValueInsert)
                {
                    CompileRemainingInsertClauses(ctx, query, table, writer, insertClauses);
                    return;
                }

                if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
                    ctx.Raw.Append(";" + LastId);

            }
        }

        protected virtual void CompileRemainingInsertClauses(SqlResult ctx, Query query, string table,
            Writer writer,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                var values = string.Join(", ", Parametrize(ctx, writer, query, insert.Values));
                ctx.Raw.Append($", ({values})");
            }
        }


        public void CompileCteQuery(Query query, Writer writer)
        {
            var cteSearchResult = CteFinder.Find(query, EngineCode);
            writer.Append("WITH ");

            foreach (var cte in cteSearchResult)
            {
                CompileCte(cte, writer);
                writer.Append(",\n");
            }

            writer.RemoveLast(2); // remove last comma
            writer.Append('\n');
        }

        /// <summary>
        ///     Compile a single column clause
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="query"></param>
        /// <param name="column"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private void CompileColumn(SqlResult ctx, Query query, AbstractColumn column, Writer writer)
        {
            if (column is RawColumn raw)
            {
                writer.AppendRaw(raw.Expression);
                ctx.BindingsAddRange(raw.Bindings);
                writer.AssertMatches(ctx);
                return;
            }

            if (column is QueryColumn queryColumn)
            {
                writer.AssertMatches(ctx);
                writer.Append("(");
                var subCtx = CompileSelectQuery(queryColumn.Query, writer);
                ctx.BindingsAddRange(subCtx.Bindings);
                writer.BindMany(subCtx.Bindings);
                writer.Pop();
                writer.Append(") ");
                writer.AppendAsAlias(queryColumn.Query.QueryAlias);
                writer.AssertMatches(ctx);
                return;
            }

            if (column is AggregatedColumn aggregatedColumn)
            {
                CompileAggregatedColumn(ctx, query, writer, aggregatedColumn);
                writer.AssertMatches(ctx);
                return;
            }

            writer.Append(XService.Wrap(((Column)column).Name));
            writer.AssertMatches(ctx);
        }

        private void CompileAggregatedColumn(SqlResult ctx, Query query, Writer writer, AggregatedColumn c)
        {
            writer.AssertMatches(ctx);
            writer.Append(c.Aggregate.ToUpperInvariant());

            var (col, alias) = XService.SplitAlias(
                XService.Wrap(c.Column.Name));

            var filterConditions = GetFilterConditions(c);

            if (!filterConditions.Any())
            {
                writer.Append("(");
                writer.Append(col);
                writer.Append(")");
                writer.Append(alias);
                writer.AssertMatches(ctx);
                return;
            }


            if (SupportsFilterClause)
            {
                writer.Append("(");
                writer.Append(col);
                writer.Append(") FILTER (WHERE ");
                CompileConditions(ctx, query, filterConditions, writer);
                writer.Append(")");
                writer.Append(alias);
                writer.AssertMatches(ctx);
                return;
            }

            writer.Append("(CASE WHEN ");
            CompileConditions(ctx, query, filterConditions, writer);
            writer.Append(" THEN ");
            writer.Append(col);
            writer.Append(" END)");
            writer.Append(alias);
            writer.AssertMatches(ctx);
        }

        private static List<AbstractCondition> GetFilterConditions(AggregatedColumn aggregatedColumn)
        {
            if (aggregatedColumn.Filter == null)
                return new List<AbstractCondition>();

            return aggregatedColumn.Filter
                .GetComponents<AbstractCondition>("where");
        }

        private void CompileCte(AbstractFrom? cte, Writer writer)
        {
            if (cte is RawFromClause raw)
            {
                writer.BindMany(raw.Bindings);
                Debug.Assert(raw.Alias != null, "raw.Alias != null");
                writer.Append($"{XService.WrapValue(raw.Alias)} AS ({XService.WrapIdentifiers(raw.Expression)})");
            }
            else if (cte is QueryFromClause queryFromClause)
            {
                var subCtx = CompileSelectQuery(queryFromClause.Query, writer.Sub());
                writer.BindMany(subCtx.Bindings);

                Debug.Assert(queryFromClause.Alias != null, "queryFromClause.Alias != null");
                writer.Append($"{XService.WrapValue(queryFromClause.Alias)} AS ({subCtx.RawSql})");
            }
            else if (cte is AdHocTableFromClause adHoc)
            {
                var subCtx = CompileAdHocQuery(adHoc, writer);

                Debug.Assert(adHoc.Alias != null, "adHoc.Alias != null");
                writer.Append($"{XService.WrapValue(adHoc.Alias)} AS ({subCtx.RawSql})");
                writer.AssertMatches(subCtx);
                writer.Pop();
            }
        }

        protected virtual string CompileColumns(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
            var aggregate = query.GetOneComponent<AggregateClause>("aggregate", EngineCode);
            if (aggregate != null)
            {
                var aggregateColumns = aggregate.Columns
                    .Select(value => XService.Wrap(value))
                    .ToList();

                if (aggregateColumns.Count == 1)
                {
                    writer.Append("SELECT ");
                    writer.AppendKeyword(aggregate.Type);
                    writer.Append("(");
                    if (query.IsDistinct)
                        writer.Append("DISTINCT ");
                    writer.List(", ", aggregateColumns);
                    writer.Append(") ");
                    writer.AppendAsAlias(aggregate.Type);
                    writer.AssertMatches(ctx);
                    return writer;
                }

                writer.Append("SELECT 1");
                writer.AssertMatches(ctx);
                return writer;
            }

            var columns = query
                .GetComponents<AbstractColumn>("select", EngineCode);

            writer.Append("SELECT ");
            if (query.IsDistinct) writer.Append("DISTINCT ");

            if (columns.Any())
            {
                writer.List(", ", columns, x => CompileColumn(ctx, query, x, writer));
            }
            else
            {
                writer.Append("*");
            }
            return writer;
        }

        private void CompileUnion(SqlResult ctx, Query query, Writer writer)
        {
            // Handle UNION, EXCEPT and INTERSECT
            if (!query.GetComponents("combine", EngineCode).Any()) return;

            var combinedQueries = new List<string>();

            var clauses = query.GetComponents<AbstractCombine>("combine", EngineCode);

            foreach (var clause in clauses)
                if (clause is Combine combineClause)
                {
                    var combineOperator = combineClause.Operation.ToUpperInvariant() + " " +
                                          (combineClause.All ? "ALL " : "");

                    var subCtx = CompileSelectQuery(combineClause.Query, writer.Sub());

                    ctx.BindingsAddRange(subCtx.Bindings);

                    combinedQueries.Add($"{combineOperator}{subCtx.RawSql}");
                }
                else
                {
                    var combineRawClause = (RawCombine)clause;

                    ctx.BindingsAddRange(combineRawClause.Bindings);

                    combinedQueries.Add(XService.WrapIdentifiers(combineRawClause.Expression));
                }

            writer.List(" ", combinedQueries);
        }

        private void CompileTableExpression(SqlResult ctx, AbstractFrom from, Writer writer)
        {
            if (from is RawFromClause raw)
            {
                writer.BindMany(raw.Bindings);
                writer.AppendRaw(raw.Expression);
                writer.AssertMatches(ctx);
                return;
            }

            if (from is QueryFromClause queryFromClause)
            {
                var q = queryFromClause.Query;
                writer.Append("(");
                CompileSelectQuery(q, writer);
                ctx.BindingsAddRange(writer.Bindings);
                writer.Pop();
                writer.AssertMatches(ctx);

                writer.Append(")");
                if (!string.IsNullOrEmpty(q.QueryAlias))
                {
                    writer.Append(" ");
                    writer.Append(TableAsKeyword);
                    writer.AppendValue(q.QueryAlias);
                }

                writer.AssertMatches(ctx);
                return;
            }

            if (from is FromClause fromClause)
            {
                writer.AppendName(fromClause.Table);
                ctx.BindingsAddRange(writer.Bindings);
                return;
            }

            throw InvalidClauseException("TableExpression", from);
        }

        private void CompileFrom(SqlResult ctx, Query query, Writer writer)
        {
            var from = query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (from == null) return;

            writer.Append("FROM ");
            CompileTableExpression(ctx, from, writer);
        }

        private string? CompileJoins(SqlResult ctx, Query query, Writer writer)
        {
            var baseJoins = query.GetComponents<BaseJoin>("join", EngineCode);
            if (!baseJoins.Any()) return null;

            writer.Append("\n");
            writer.List("\n", baseJoins, x => CompileJoin(ctx, query, x.Join, writer));
            return writer;
        }

        private void CompileJoin(SqlResult ctx, Query query, Join join, Writer writer)
        {
            var from = join.BaseQuery.GetOneComponent<AbstractFrom>("from", EngineCode);
            var conditions = join.BaseQuery.GetComponents<AbstractCondition>("where", EngineCode);

            Debug.Assert(from != null, nameof(from) + " != null");

            writer.Append(join.Type);
            writer.Append(" ");
            CompileTableExpression(ctx, from, writer);

            if (conditions.Any())
            {
                writer.Append(" ON ");
                CompileConditions(ctx, query, conditions, writer);
            }
        }

        private string? CompileWheres(SqlResult ctx, Query query, Writer writer)
        {
            var conditions = query.GetComponents<AbstractCondition>("where", EngineCode);
            if (!conditions.Any()) return null;

            writer.Append("WHERE ");
            CompileConditions(ctx, query, conditions, writer);
            return writer;
        }

        private void CompileGroups(SqlResult ctx, Query query, Writer writer)
        {
            var components = query.GetComponents<AbstractColumn>("group", EngineCode);
            if (!components.Any()) return;
            writer.Append("GROUP BY ");
            writer.List(", ", components, x => CompileColumn(ctx, query, x, writer));
        }

        protected string? CompileOrders(SqlResult ctx, Query query, Writer writer)
        {
            if (!query.HasComponent("order", EngineCode)) return null;

            var columns = query
                .GetComponents<AbstractOrderBy>("order", EngineCode)
                .Select(x =>
                {
                    if (x is RawOrderBy raw)
                    {
                        ctx.BindingsAddRange(raw.Bindings);
                        return XService.WrapIdentifiers(raw.Expression);
                    }

                    var direction = ((OrderBy)x).Ascending ? "" : " DESC";

                    return XService.Wrap(((OrderBy)x).Column) + direction;
                });

            writer.Append("ORDER BY ");
            writer.List(", ", columns);
            return writer;
        }

        private void CompileHaving(SqlResult ctx, Query query, Writer writer)
        {
            var havingClauses = query.GetComponents("having", EngineCode);
            if (havingClauses.Count == 0) return;

            writer.Append("HAVING ");
            CompileConditions(ctx,query, 
                havingClauses.Cast<AbstractCondition>().ToList(),
                writer);
        }

        protected virtual string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            if (limit != 0)
            {
                ctx.BindingsAdd(limit);
                writer.Append("LIMIT ?");
            }

            var offset = query.GetOffset(EngineCode);
            if (offset != 0)
            {
                ctx.BindingsAdd(offset);
                writer.Whitespace();
                writer.Append("OFFSET ?");
            }
            return writer;
        }

        protected virtual string CompileTrue()
        {
            return "true";
        }

        protected virtual string CompileFalse()
        {
            return "false";
        }

        private InvalidCastException InvalidClauseException(string section, AbstractClause clause)
        {
            return new InvalidCastException(
                $"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");
        }


        protected static object? Resolve(Query query, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal) return literal.Value;

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
                return query.FindVariable(variable.Name);

            return parameter;
        }

        protected static string Parameter(SqlResult ctx, Query query, Writer writer, object? parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal) return literal.Value;

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
            {
                var value = query.FindVariable(variable.Name);
                ctx.BindingsAdd(value);
                writer.BindOne(value);
                return "?";
            }

            ctx.BindingsAdd(parameter);
            writer.BindOne(parameter);
            return "?";
        }

        protected string Parametrize(SqlResult ctx, Writer writer, Query query, IEnumerable<object> values)
        {
            return string.Join(", ", values.Select(x => Parameter(ctx, query, writer, x)));
        }
    }
}
