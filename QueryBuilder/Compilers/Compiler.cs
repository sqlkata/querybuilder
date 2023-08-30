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


        protected const string SingleInsertStartClause = "INSERT INTO";
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

        protected SqlResult CompileSelectQuery(Query query, Writer writer)
        {
            var ctx = new SqlResult();
            writer.Push(ctx);
            CompileSelectQueryInner(ctx, query, writer);

            return ctx;

        }

        public virtual void CompileSelectQueryInner(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
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
            writer.AssertMatches(ctx);
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
            if (!query.HasComponent("join", EngineCode))
            {
                writer.Append("DELETE FROM ");
                WriteTable(ctx, query, writer, "delete");
                CompileWheres(ctx, query, writer);
            }
            else
            {
                var fromClause = query.GetOneComponent<AbstractFrom>("from", EngineCode);
                if (fromClause is not FromClause c) return;

                writer.Append("DELETE ");
                writer.AppendName(c.Alias);
                writer.Append(" FROM ");
                WriteTable(ctx, fromClause, writer, "delete");
                CompileJoins(ctx, query, writer);
                CompileWheres(ctx, query, writer);
            }
            ctx.Raw.Append(writer);
        }

        public void CompileUpdateQuery(SqlResult ctx, Query query, Writer writer)
        {
            writer.Append("UPDATE ");

            WriteTable(ctx, query, writer, "update");

            var clause = query.GetOneComponent("update", EngineCode);
            if (clause is IncrementClause increment)
            {
                CompileIncrement(increment);
                return;
            }

            var toUpdate = query.GetOneComponent<InsertClause>("update", EngineCode);
            Debug.Assert(toUpdate != null);
            CompileUpdate(toUpdate);

            void CompileIncrement(IncrementClause incrementClause)
            {
                writer.Append(" SET ");
                writer.AppendName(incrementClause.Column);
                writer.Append(" = ");
                writer.AppendName(incrementClause.Column);
                writer.Append(" ");
                writer.Append(incrementClause.Value >= 0 ? "+" : "-");
                writer.Append(" ");
                writer.Append(Parameter(ctx, query, writer, Math.Abs(incrementClause.Value)));
                CompileWheres(ctx, query, writer);
                ctx.Raw.Append(writer);
            }

            void CompileUpdate(InsertClause insertClause)
            {
                writer.Append(" SET ");
                writer.List(", ", insertClause.Columns, (column, i) =>
                {
                    writer.AppendName(column);
                    writer.Append(" = ");
                    writer.Append(Parameter(ctx, query, writer, insertClause.Values[i]));
                });
                CompileWheres(ctx, query, writer);
                ctx.Raw.Append(writer);
            }
        }

        public virtual void CompileInsertQuery(SqlResult ctx, Query query, Writer writer)
        {
            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            var isMultiValueInsert = inserts.OfType<InsertClause>().Skip(1).Any();

            writer.Append(isMultiValueInsert
                ? MultiInsertStartClause
                : SingleInsertStartClause);
            writer.Append(" ");
            var table = WriteTable(ctx, query, writer, "insert");
            writer.AssertMatches(ctx);

            if (inserts[0] is InsertQueryClause insertQueryClause)
            {

                CompileInsertQueryClause(insertQueryClause, writer);
                writer.AssertMatches(ctx);
                return;
            }

            CompileValueInsertClauses();
            writer.AssertMatches(ctx);
            return;


            void CompileInsertQueryClause(InsertQueryClause clause, Writer w)
            {
                w.WriteInsertColumnsList(clause.Columns);
                w.Append(" ");

                var subCtx = CompileSelectQuery(clause.Query, w);
                ctx.BindingsAddRange(subCtx.Bindings);
              //  w.BindMany(subCtx.Bindings);
                w.Pop();
                w.AssertMatches(ctx);

                ctx.Raw.Append(w);
            }

            void CompileValueInsertClauses()
            {
                var insertClauses = inserts.Cast<InsertClause>().ToArray();
                var firstInsert = insertClauses.First();
                writer.WriteInsertColumnsList(firstInsert.Columns);
                writer.Append(" VALUES (");
                writer.List(", ", firstInsert.Values, p =>
                {
                    writer.Append(Parameter(ctx, query, writer, p));
                });
                writer.Append(")");

                if (isMultiValueInsert)
                {
                    CompileRemainingInsertClauses(ctx,
                        query, table, writer, insertClauses);
                    ctx.Raw.Append(writer);

                    return;
                }
                if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
                {
                    writer.Append(";");
                    writer.Append(LastId);
                }
                ctx.Raw.Append(writer);
                writer.AssertMatches(ctx);
            }
        }

        protected virtual void CompileRemainingInsertClauses(SqlResult ctx, Query query, string table,
            Writer writer,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                writer.Append(", (");
                writer.List(", ", insert.Values, value =>
                {
                    writer.Append(Parameter(ctx, query, writer, value));
                });
                writer.Append(")");
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
                Debug.Assert(queryFromClause.Alias != null, "queryFromClause.Alias != null");
                writer.AppendValue(queryFromClause.Alias);
                writer.Append(" AS (");
                CompileSelectQuery(queryFromClause.Query, writer);
                writer.Pop();
                writer.Append(")");
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
            writer.Append("SELECT ");
            return CompileColumnsAfterSelect(ctx, query, writer);
        }

        protected string CompileColumnsAfterSelect(SqlResult ctx, Query query, Writer writer)
        {
            var aggregate = query.GetOneComponent<AggregateClause>("aggregate", EngineCode);
            if (aggregate != null)
            {
                CompileAggregateColumns();
            }
            else
            {
                if (query.IsDistinct) writer.Append("DISTINCT ");
                CompileFlatColumns(query, writer, ctx);
            }
            return writer;

            void CompileAggregateColumns()
            {
                var aggregateColumns = aggregate.Columns
                    .Select(value => XService.Wrap(value))
                    .ToList();

                if (aggregateColumns.Count == 1)
                {
                    writer.AppendKeyword(aggregate.Type);
                    writer.Append("(");
                    if (query.IsDistinct)
                        writer.Append("DISTINCT ");
                    writer.List(", ", aggregateColumns);
                    writer.Append(") ");
                    writer.AppendAsAlias(aggregate.Type);
                    writer.AssertMatches(ctx);
                }
                else
                {
                    writer.Append("1");
                    writer.AssertMatches(ctx);
                }
            }
        }

        protected void CompileFlatColumns(Query query, Writer writer, SqlResult ctx)
        {
            var columns = query
                .GetComponents<AbstractColumn>("select", EngineCode);
            if (columns.Any())
            {
                writer.List(", ", columns, x => CompileColumn(ctx, query, x, writer));
            }
            else
            {
                writer.Append("*");
            }
        }

        private void CompileUnion(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
            // Handle UNION, EXCEPT and INTERSECT
            writer.List(" ",
                query.GetComponents<AbstractCombine>("combine", EngineCode),
                clause =>
                {
                    if (clause is Combine combine)
                    {
                        writer.AppendKeyword(combine.Operation);
                        writer.Append(" ");
                        if (combine.All)
                            writer.Append("ALL ");

                        writer.AssertMatches(ctx);
                        ctx.BindingsAddRange(
                            CompileSelectQuery(combine.Query, writer).Bindings);
                        writer.Pop();

                    }
                    else if (clause is RawCombine combineRawClause)
                    {
                        writer.AppendRaw(combineRawClause.Expression);
                        ctx.BindingsAddRange(combineRawClause.Bindings);
                    }
                });
            writer.AssertMatches(ctx);
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
                return;
            }

            throw InvalidClauseException("TableExpression", from);
        }

        protected string WriteTable(SqlResult sqlResult, Query query, Writer writer, string operationName)
        {
            return WriteTable(sqlResult,
                query.GetOneComponent<AbstractFrom>("from", EngineCode),
                writer, operationName);
        }
        protected static string WriteTable(SqlResult sqlResult, AbstractFrom? abstractFrom, Writer writer, string operationName)
        {
            switch (abstractFrom)
            {
                case null:
                    throw new InvalidOperationException($"No table set to {operationName}");

                case FromClause fromClauseCast:
                    writer.AppendName(fromClauseCast.Table);
                    return writer.X.Wrap(fromClauseCast.Table);
                case RawFromClause rawFromClause:
                    {
                        writer.AppendRaw(rawFromClause.Expression);
                        if (rawFromClause.Bindings.Length > 0)
                        {
                            //TODO: test!
                            sqlResult.BindingsAddRange(rawFromClause.Bindings);
                            writer.BindMany(rawFromClause.Bindings);
                        }
                        return writer.X.WrapIdentifiers(rawFromClause.Expression);
                    }
                default:
                    throw new InvalidOperationException("Invalid table expression");
            }
        }

        private void CompileFrom(SqlResult ctx, Query query, Writer writer)
        {
            var from = query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (from == null) return;

            writer.Append("FROM ");
            CompileTableExpression(ctx, from, writer);
            writer.AssertMatches(ctx);
        }

        private void CompileJoins(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
            var baseJoins = query.GetComponents<BaseJoin>("join", EngineCode);
            if (!baseJoins.Any())
            {
                writer.AssertMatches(ctx);
                return;
            }

            writer.Whitespace();
            writer.Append("\n");
            writer.List("\n", baseJoins, x => CompileJoin(ctx, query, x.Join, writer));
            writer.AssertMatches(ctx);
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

        private void CompileWheres(SqlResult ctx, Query query, Writer writer)
        {
            var conditions = query.GetComponents<AbstractCondition>("where", EngineCode);
            if (!conditions.Any()) return;

            writer.Whitespace();
            writer.Append("WHERE ");
            CompileConditions(ctx, query, conditions, writer);
            writer.AssertMatches(ctx);
        }

        private void CompileGroups(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
            var components = query.GetComponents<AbstractColumn>("group", EngineCode);
            if (!components.Any())
            {
                writer.AssertMatches(ctx);
                return;
            }
            writer.Append("GROUP BY ");
            writer.List(", ", components, x => CompileColumn(ctx, query, x, writer));
            writer.AssertMatches(ctx);
        }

        protected string? CompileOrders(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
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
            writer.AssertMatches(ctx);
            return writer;
        }

        private void CompileHaving(SqlResult ctx, Query query, Writer writer)
        {
            var havingClauses = query.GetComponents("having", EngineCode);
            if (havingClauses.Count == 0) return;

            writer.Append("HAVING ");
            CompileConditions(ctx, query,
                havingClauses.Cast<AbstractCondition>().ToList(),
                writer);
            writer.AssertMatches(ctx);
        }

        protected virtual string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            writer.AssertMatches(ctx);
            var limit = query.GetLimit(EngineCode);
            if (limit != 0)
            {
                ctx.BindingsAdd(limit);
                writer.BindOne(limit);
                writer.Append("LIMIT ?");
            }

            var offset = query.GetOffset(EngineCode);
            if (offset != 0)
            {
                ctx.BindingsAdd(offset);
                writer.BindOne(offset);
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

        public static string Parameter(SqlResult ctx, Query query, Writer writer, object? parameter)
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
