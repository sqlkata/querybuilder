using System.Diagnostics;
using System.Text;

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

        public virtual SqlResult CompileSelectQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query.Clone()
            };
            var writer = new Writer(XService);
            writer.WhitespaceSeparated(
                () => CompileColumns(ctx, writer),
                () => CompileFrom(ctx, writer),
                () => CompileJoins(ctx, writer),
                () => CompileWheres(ctx, writer),
                () => CompileGroups(ctx, writer),
                () => CompileHaving(ctx, writer),
                () => CompileOrders(ctx, writer),
                () => CompileLimit(ctx, writer),
                () => CompileUnion(ctx, writer));
           
            ctx.Raw.Append(writer);

            return ctx;
        }

        protected virtual SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
        {
            var ctx = new SqlResult { Query = null };

            var row = "SELECT " +
                      string.Join(", ", adHoc.Columns.Select(col => $"? AS {XService.Wrap(col)}"));

            var fromTable = SingleRowDummyTableName;

            if (fromTable != null) row += $" FROM {fromTable}";

            var rows = string.Join(" UNION ALL ", Enumerable.Repeat(row, adHoc.Values.Length / adHoc.Columns.Length));

            ctx.Raw.Append(rows);
            ctx.Bindings = adHoc.Values.ToList();

            return ctx;
        }

        public SqlResult CompileDeleteQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
                throw new InvalidOperationException("No table set to delete");

            var fromClause = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            string? table = null;

            if (fromClause is FromClause fromClauseCast) table = XService.Wrap(fromClauseCast.Table);

            if (fromClause is RawFromClause rawFromClause)
            {
                table = XService.WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null) throw new InvalidOperationException("Invalid table expression");

            var joins = CompileJoins(ctx, new Writer(XService));

            var where = CompileWheres(ctx, new Writer(XService));

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

            return ctx;
        }

        public SqlResult CompileUpdateQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
                throw new InvalidOperationException("No table set to update");

            var fromClause = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            string? table = null;

            if (fromClause is FromClause fromClauseCast) table = XService.Wrap(fromClauseCast.Table);

            if (fromClause is RawFromClause rawFromClause)
            {
                table = XService.WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null) throw new InvalidOperationException("Invalid table expression");

            // check for increment statements
            var clause = ctx.Query.GetOneComponent("update", EngineCode);

            string? wheres;

            if (clause is IncrementClause increment)
            {
                var column = XService.Wrap(increment.Column);
                var value = Parameter(ctx, Math.Abs(increment.Value));
                var sign = increment.Value >= 0 ? "+" : "-";

                wheres = CompileWheres(ctx, new Writer(XService));

                if (!string.IsNullOrEmpty(wheres)) wheres = " " + wheres;

                ctx.Raw.Append($"UPDATE {table} SET {column} = {column} {sign} {value}{wheres}");

                return ctx;
            }


            var toUpdate = ctx.Query.GetOneComponent<InsertClause>("update", EngineCode);
            Debug.Assert(toUpdate != null);
            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Length; i++)
                parts.Add(XService.Wrap(toUpdate.Columns[i]) + " = " + Parameter(ctx, toUpdate.Values[i]));

            var sets = string.Join(", ", parts);

            wheres = CompileWheres(ctx, new Writer(XService));

            if (!string.IsNullOrEmpty(wheres)) wheres = " " + wheres;

            ctx.Raw.Append($"UPDATE {table} SET {sets}{wheres}");

            return ctx;
        }

        public virtual SqlResult CompileInsertQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
                throw new InvalidOperationException("No table set to insert");

            var fromClause = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (fromClause is null)
                throw new InvalidOperationException("Invalid table expression");

            string? table = null;
            if (fromClause is FromClause fromClauseCast)
                table = XService.Wrap(fromClauseCast.Table);
            if (fromClause is RawFromClause rawFromClause)
            {
                table = XService.WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null)
                throw new InvalidOperationException("Invalid table expression");

            var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            if (inserts[0] is InsertQueryClause insertQueryClause)
                return CompileInsertQueryClause(insertQueryClause);
            return CompileValueInsertClauses(inserts.Cast<InsertClause>().ToArray());


            SqlResult CompileInsertQueryClause(InsertQueryClause clause)
            {
                var columns = clause.Columns.GetInsertColumnsList(XService);

                var subCtx = CompileSelectQuery(clause.Query);
                ctx.Bindings.AddRange(subCtx.Bindings);

                ctx.Raw.Append($"{SingleInsertStartClause} {table}{columns} {subCtx.RawSql}");

                return ctx;
            }

            SqlResult CompileValueInsertClauses(InsertClause[] insertClauses)
            {
                var isMultiValueInsert = insertClauses.Length > 1;

                var insertInto = isMultiValueInsert ? MultiInsertStartClause : SingleInsertStartClause;

                var firstInsert = insertClauses.First();
                var columns = firstInsert.Columns.GetInsertColumnsList(XService);
                var values = string.Join(", ", Parametrize(ctx, firstInsert.Values));

                ctx.Raw.Append($"{insertInto} {table}{columns} VALUES ({values})");

                if (isMultiValueInsert)
                    return CompileRemainingInsertClauses(ctx, table, insertClauses);

                if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
                    ctx.Raw.Append(";" + LastId);

                return ctx;
            }
        }

        protected virtual SqlResult CompileRemainingInsertClauses(SqlResult ctx, string table,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                var values = string.Join(", ", Parametrize(ctx, insert.Values));
                ctx.Raw.Append($", ({values})");
            }

            return ctx;
        }


        public SqlResult CompileCteQuery(SqlResult ctx, Query query)
        {
            var cteSearchResult = CteFinder.Find(query, EngineCode);

            var rawSql = new StringBuilder("WITH ");
            var cteBindings = new List<object?>();

            foreach (var cte in cteSearchResult)
            {
                var cteCtx = CompileCte(cte);

                cteBindings.AddRange(cteCtx.Bindings);
                rawSql.Append(cteCtx.RawSql.Trim());
                rawSql.Append(",\n");
            }

            rawSql.Length -= 2; // remove last comma
            rawSql.Append('\n');
            rawSql.Append(ctx.RawSql);

            ctx.Bindings.InsertRange(0, cteBindings);
            ctx.ReplaceRaw(rawSql.ToString());

            return ctx;
        }

        /// <summary>
        ///     Compile a single column clause
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="column"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private void CompileColumn(SqlResult ctx, AbstractColumn column, Writer writer)
        {
            if (column is RawColumn raw)
            {
                writer.AppendRaw(raw.Expression);
                ctx.Bindings.AddRange(raw.Bindings);
                return;
            }

            if (column is QueryColumn queryColumn)
            {
                var alias = XService.AsAlias(queryColumn.Query.QueryAlias);
                var subCtx = CompileSelectQuery(queryColumn.Query);

                ctx.Bindings.AddRange(subCtx.Bindings);

                writer.S.Append("(");
                writer.S.Append(subCtx.RawSql);
                writer.S.Append(")");
                writer.S.Append(alias);
                return;
            }

            if (column is AggregatedColumn aggregatedColumn)
            {
                var agg = aggregatedColumn.Aggregate.ToUpperInvariant();
                var filterCondition = CompileFilterConditions(ctx, aggregatedColumn);

                var sub = writer.Sub();
                CompileColumn(ctx, aggregatedColumn.Column, sub);
                var (col, alias) = XService.SplitAlias(sub);

                if (string.IsNullOrEmpty(filterCondition))
                {
                    writer.S.Append(agg);
                    writer.S.Append("(");
                    writer.S.Append(col);
                    writer.S.Append(")");
                    writer.S.Append(alias);
                    return;
                }

                if (SupportsFilterClause)
                {
                    writer.S.Append($"{agg}({col}) FILTER (WHERE {filterCondition}){alias}");
                    return;
                }

                writer.S.Append($"{agg}(CASE WHEN {filterCondition} THEN {col} END){alias}");

                return;
            }

            writer.S.Append(XService.Wrap(((Column)column).Name));
        }

        private string? CompileFilterConditions(SqlResult ctx, AggregatedColumn aggregatedColumn)
        {
            if (aggregatedColumn.Filter == null) return null;

            var wheres = aggregatedColumn.Filter.GetComponents<AbstractCondition>("where");

            return CompileConditions(ctx, wheres);
        }

        private SqlResult CompileCte(AbstractFrom? cte)
        {
            var ctx = new SqlResult { Query = null };

            if (cte == null) return ctx;

            if (cte is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                Debug.Assert(raw.Alias != null, "raw.Alias != null");
                ctx.Raw.Append($"{XService.WrapValue(raw.Alias)} AS ({XService.WrapIdentifiers(raw.Expression)})");
            }
            else if (cte is QueryFromClause queryFromClause)
            {
                var subCtx = CompileSelectQuery(queryFromClause.Query);
                ctx.Bindings.AddRange(subCtx.Bindings);

                Debug.Assert(queryFromClause.Alias != null, "queryFromClause.Alias != null");
                ctx.Raw.Append($"{XService.WrapValue(queryFromClause.Alias)} AS ({subCtx.RawSql})");
            }
            else if (cte is AdHocTableFromClause adHoc)
            {
                var subCtx = CompileAdHocQuery(adHoc);
                ctx.Bindings.AddRange(subCtx.Bindings);

                Debug.Assert(adHoc.Alias != null, "adHoc.Alias != null");
                ctx.Raw.Append($"{XService.WrapValue(adHoc.Alias)} AS ({subCtx.RawSql})");
            }

            return ctx;
        }

        protected virtual string CompileColumns(SqlResult ctx, Writer writer)
        {
            var aggregate = ctx.Query.GetOneComponent<AggregateClause>("aggregate", EngineCode);
            if (aggregate != null)
            {
                var aggregateColumns = aggregate.Columns
                    .Select(value => XService.Wrap(value))
                    .ToList();

                if (aggregateColumns.Count == 1)
                {
                    writer.S.Append("SELECT ");
                    writer.AppendKeyword(aggregate.Type);
                    writer.S.Append("(");
                    if (ctx.Query.IsDistinct)
                        writer.S.Append("DISTINCT ");
                    writer.List(", ", aggregateColumns);
                    writer.S.Append(") ");
                    writer.AppendAsAlias(aggregate.Type);
                    return writer;
                }

                writer.S.Append("SELECT 1");
                return writer;
            }

            var columns = ctx.Query
                .GetComponents<AbstractColumn>("select", EngineCode);
            // .Select(x => CompileColumn(ctx, x, writer))
            // .ToList();

            writer.S.Append("SELECT ");
            if (ctx.Query.IsDistinct) writer.S.Append("DISTINCT ");

            if (columns.Any())
            {
                writer.List(", ", columns, x => CompileColumn(ctx, x, writer));
            }
            else
            {
                writer.S.Append("*");
            }
            return writer;
        }

        private void CompileUnion(SqlResult ctx, Writer writer)
        {
            // Handle UNION, EXCEPT and INTERSECT
            if (!ctx.Query.GetComponents("combine", EngineCode).Any()) return;

            var combinedQueries = new List<string>();

            var clauses = ctx.Query.GetComponents<AbstractCombine>("combine", EngineCode);

            foreach (var clause in clauses)
                if (clause is Combine combineClause)
                {
                    var combineOperator = combineClause.Operation.ToUpperInvariant() + " " +
                                          (combineClause.All ? "ALL " : "");

                    var subCtx = CompileSelectQuery(combineClause.Query);

                    ctx.Bindings.AddRange(subCtx.Bindings);

                    combinedQueries.Add($"{combineOperator}{subCtx.RawSql}");
                }
                else
                {
                    var combineRawClause = (RawCombine)clause;

                    ctx.Bindings.AddRange(combineRawClause.Bindings);

                    combinedQueries.Add(XService.WrapIdentifiers(combineRawClause.Expression));
                }

            writer.List(" ", combinedQueries);
        }

        private void CompileTableExpression(SqlResult ctx, AbstractFrom from, Writer writer)
        {
            if (from is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                writer.AppendRaw(raw.Expression);
                return;
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;

                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias)
                    ? ""
                    : $" {TableAsKeyword}" + XService.WrapValue(fromQuery.QueryAlias);

                var subCtx = CompileSelectQuery(fromQuery);

                ctx.Bindings.AddRange(subCtx.Bindings);
                writer.S.Append("(" + subCtx.RawSql + ")" + alias);
                return;
            }

            if (from is FromClause fromClause)
            {
                writer.AppendName(fromClause.Table);
                return;
            }

            throw InvalidClauseException("TableExpression", from);
        }

        private void CompileFrom(SqlResult ctx, Writer writer)
        {
            var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (from == null) return;

            writer.S.Append("FROM ");
            CompileTableExpression(ctx, from, writer);
        }

        private string? CompileJoins(SqlResult ctx, Writer writer)
        {
            var baseJoins = ctx.Query.GetComponents<BaseJoin>("join", EngineCode);
            if (!baseJoins.Any()) return null;

            writer.S.Append("\n");
            writer.List("\n", baseJoins, x => CompileJoin(ctx, x.Join, writer));
            return writer; 
        }

        private void CompileJoin(SqlResult ctx, Join join, Writer writer)
        {
            var from = join.BaseQuery.GetOneComponent<AbstractFrom>("from", EngineCode);
            var conditions = join.BaseQuery.GetComponents<AbstractCondition>("where", EngineCode);

            Debug.Assert(from != null, nameof(from) + " != null");
            var constraints = CompileConditions(ctx, conditions);

            var onClause = conditions.Any() ? $" ON {constraints}" : "";

            writer.S.Append(join.Type);
            writer.S.Append(" ");
            CompileTableExpression(ctx, from, writer);
            writer.S.Append(onClause);
        }

        private string? CompileWheres(SqlResult ctx, Writer writer)
        {
            var conditions = ctx.Query.GetComponents<AbstractCondition>("where", EngineCode);
            if (!conditions.Any()) return null;

            var sql = CompileConditions(ctx, conditions).Trim();
            if (string.IsNullOrEmpty(sql)) return null;
            writer.S.Append("WHERE ");
            writer.S.Append(sql);
            return writer;
        }

        private void CompileGroups(SqlResult ctx, Writer writer)
        {
            var components = ctx.Query.GetComponents<AbstractColumn>("group", EngineCode);
            if (!components.Any()) return;
            writer.S.Append("GROUP BY ");
            writer.List(", ", components, x => CompileColumn(ctx, x, writer));
        }

        protected string? CompileOrders(SqlResult ctx, Writer writer)
        {
            if (!ctx.Query.HasComponent("order", EngineCode)) return null;

            var columns = ctx.Query
                .GetComponents<AbstractOrderBy>("order", EngineCode)
                .Select(x =>
                {
                    if (x is RawOrderBy raw)
                    {
                        ctx.Bindings.AddRange(raw.Bindings);
                        return XService.WrapIdentifiers(raw.Expression);
                    }

                    var direction = ((OrderBy)x).Ascending ? "" : " DESC";

                    return XService.Wrap(((OrderBy)x).Column) + direction;
                });

            writer.S.Append("ORDER BY ");
            writer.List(", ", columns);
            return writer;
        }

        private void CompileHaving(SqlResult ctx, Writer writer)
        {
            if (!ctx.Query.HasComponent("having", EngineCode)) return;

            var sql = new List<string>();

            var having = ctx.Query.GetComponents("having", EngineCode)
                .Cast<AbstractCondition>()
                .ToList();

            for (var i = 0; i < having.Count; i++)
            {
                var compiled = CompileCondition(ctx, having[i]);

                if (!string.IsNullOrEmpty(compiled))
                {
                    var boolOperator = i > 0 ? having[i].IsOr ? "OR " : "AND " : "";

                    sql.Add(boolOperator + compiled);
                }
            }

            writer.S.Append("HAVING ");
            writer.List(" ", sql);
        }

        protected virtual string? CompileLimit(SqlResult ctx, Writer writer)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            if (limit != 0)
            {
                ctx.Bindings.Add(limit);
                writer.S.Append("LIMIT ?");
            }

            var offset = ctx.Query.GetOffset(EngineCode);
            if (offset != 0)
            {
                ctx.Bindings.Add(offset);
                writer.Whitespace();
                writer.S.Append("OFFSET ?");
            }
            return writer;
        }

        private string CompileLower(string value)
        {
            return $"LOWER({value})";
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


        /// <summary>
        ///     Resolve a parameter
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected static object? Resolve(SqlResult ctx, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal) return literal.Value;

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
                return ctx.Query.FindVariable(variable.Name);

            return parameter;
        }

        /// <summary>
        ///     Resolve a parameter and add it to the binding list
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected static string Parameter(SqlResult ctx, object? parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal) return literal.Value;

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
            {
                var value = ctx.Query.FindVariable(variable.Name);
                ctx.Bindings.Add(value);
                return "?";
            }

            ctx.Bindings.Add(parameter);
            return "?";
        }

        /// <summary>
        ///     Create query parameter place-holders for an array.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        protected string Parametrize(SqlResult ctx, IEnumerable<object> values)
        {
            return string.Join(", ", values.Select(x => Parameter(ctx, x)));
        }


    }
}
