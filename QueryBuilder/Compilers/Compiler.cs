using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private readonly ConditionsCompilerProvider _compileConditionMethodsProvider;

        /// <summary>
        ///     A list of white-listed operators
        /// </summary>
        /// <value></value>
        private readonly HashSet<string> _operators = new()
        {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "not like",
            "ilike", "not ilike",
            "like binary", "not like binary",
            "rlike", "not rlike",
            "regexp", "not regexp",
            "similar to", "not similar to"
        };

        private readonly HashSet<string> _userOperators = new();


        protected Compiler()
        {
            _compileConditionMethodsProvider = new ConditionsCompilerProvider(this);
        }

        public const string ParameterPlaceholder = "?";
        public string ParameterPrefix { get; init; } = "@p";
        protected string OpeningIdentifier { get; init; } = "\"";
        protected string ClosingIdentifier { get; init; } = "\"";
        protected string ColumnAsKeyword { get; init; } = "AS ";
        protected string TableAsKeyword { get; init; } = "AS ";
        protected string LastId { get; init; } = "";
        private const string EscapeCharacter = "\\";


        private const string SingleInsertStartClause = "INSERT INTO";
        protected string MultiInsertStartClause { get; init; } = "INSERT INTO";
        public string? EngineCode { get; protected init; }
        protected string? SingleRowDummyTableName { get; init; }

        /// <summary>
        ///     Whether the compiler supports the `SELECT ... FILTER` syntax
        /// </summary>
        /// <value></value>
        public bool SupportsFilterClause { get; init; }

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
            foreach (var op in operators) _userOperators.Add(op);

            return this;
        }
        
        public virtual SqlResult CompileSelectQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query.Clone()
            };

            var results = new[]
                {
                    CompileColumns(ctx),
                    CompileFrom(ctx),
                    CompileJoins(ctx),
                    CompileWheres(ctx),
                    CompileGroups(ctx),
                    CompileHaving(ctx),
                    CompileOrders(ctx),
                    CompileLimit(ctx),
                    CompileUnion(ctx)
                }
                .Where(x => x != null)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var sql = string.Join(" ", results);

            ctx.RawSql = sql;

            return ctx;
        }

        protected virtual SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
        {
            var ctx = new SqlResult { Query = null };

            var row = "SELECT " +
                      string.Join(", ", adHoc.Columns.Select(col => $"{ParameterPlaceholder} AS {Wrap(col)}"));

            var fromTable = SingleRowDummyTableName;

            if (fromTable != null) row += $" FROM {fromTable}";

            var rows = string.Join(" UNION ALL ", Enumerable.Repeat(row, adHoc.Values.Length / adHoc.Columns.Length));

            ctx.RawSql = rows;
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

            if (fromClause is FromClause fromClauseCast) table = Wrap(fromClauseCast.Table);

            if (fromClause is RawFromClause rawFromClause)
            {
                table = WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null) throw new InvalidOperationException("Invalid table expression");

            var joins = CompileJoins(ctx);

            var where = CompileWheres(ctx);

            if (!string.IsNullOrEmpty(where)) where = " " + where;

            if (string.IsNullOrEmpty(joins))
            {
                ctx.RawSql = $"DELETE FROM {table}{where}";
            }
            else
            {
                // check if we have alias 
                if (fromClause is FromClause && !string.IsNullOrEmpty(fromClause.Alias))
                    ctx.RawSql = $"DELETE {Wrap(fromClause.Alias)} FROM {table} {joins}{where}";
                else
                    ctx.RawSql = $"DELETE {table} FROM {table} {joins}{where}";
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

            if (fromClause is FromClause fromClauseCast) table = Wrap(fromClauseCast.Table);

            if (fromClause is RawFromClause rawFromClause)
            {
                table = WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null) throw new InvalidOperationException("Invalid table expression");

            // check for increment statements
            var clause = ctx.Query.GetOneComponent("update", EngineCode);

            string? wheres;

            if (clause is IncrementClause increment)
            {
                var column = Wrap(increment.Column);
                var value = Parameter(ctx, Math.Abs(increment.Value));
                var sign = increment.Value >= 0 ? "+" : "-";

                wheres = CompileWheres(ctx);

                if (!string.IsNullOrEmpty(wheres)) wheres = " " + wheres;

                ctx.RawSql = $"UPDATE {table} SET {column} = {column} {sign} {value}{wheres}";

                return ctx;
            }


            var toUpdate = ctx.Query.GetOneComponent<InsertClause>("update", EngineCode);
            Debug.Assert(toUpdate != null);
            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Length; i++)
                parts.Add(Wrap(toUpdate.Columns[i]) + " = " + Parameter(ctx, toUpdate.Values[i]));

            var sets = string.Join(", ", parts);

            wheres = CompileWheres(ctx);

            if (!string.IsNullOrEmpty(wheres)) wheres = " " + wheres;

            ctx.RawSql = $"UPDATE {table} SET {sets}{wheres}";

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
                table = Wrap(fromClauseCast.Table);
            if (fromClause is RawFromClause rawFromClause)
            {
                table = WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null)
                throw new InvalidOperationException("Invalid table expression");

            var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            if (inserts[0] is InsertQueryClause insertQueryClause)
                return CompileInsertQueryClause(ctx, table, insertQueryClause);
            return CompileValueInsertClauses(ctx, table, inserts.Cast<InsertClause>().ToArray());
        }

        protected SqlResult CompileInsertQueryClause(
            SqlResult ctx, string table, InsertQueryClause clause)
        {
            var columns = GetInsertColumnsList(clause.Columns);

            var subCtx = CompileSelectQuery(clause.Query);
            ctx.Bindings.AddRange(subCtx.Bindings);

            ctx.RawSql = $"{SingleInsertStartClause} {table}{columns} {subCtx.RawSql}";

            return ctx;
        }

        protected SqlResult CompileValueInsertClauses(
            SqlResult ctx, string table, InsertClause[] insertClauses)
        {
            var isMultiValueInsert = insertClauses.Length > 1;

            var insertInto = isMultiValueInsert ? MultiInsertStartClause : SingleInsertStartClause;

            var firstInsert = insertClauses.First();
            var columns = GetInsertColumnsList(firstInsert.Columns);
            var values = string.Join(", ", Parametrize(ctx, firstInsert.Values));

            ctx.RawSql = $"{insertInto} {table}{columns} VALUES ({values})";

            if (isMultiValueInsert)
                return CompileRemainingInsertClauses(ctx, table, insertClauses);

            if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
                ctx.RawSql += ";" + LastId;

            return ctx;
        }

        protected virtual SqlResult CompileRemainingInsertClauses(SqlResult ctx, string table,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                var values = string.Join(", ", Parametrize(ctx, insert.Values));
                ctx.RawSql += $", ({values})";
            }

            return ctx;
        }

        protected string GetInsertColumnsList(ImmutableArray<string> columnList)
        {
            return !columnList.Any()
                ? ""
                : $" ({string.Join(", ", columnList.Select(Wrap))})";
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
            ctx.RawSql = rawSql.ToString();

            return ctx;
        }

        /// <summary>
        ///     Compile a single column clause
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string CompileColumn(SqlResult ctx, AbstractColumn column)
        {
            if (column is RawColumn raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (column is QueryColumn queryColumn)
            {
                var alias = "";

                if (!string.IsNullOrWhiteSpace(queryColumn.Query.QueryAlias))
                    alias = $" {ColumnAsKeyword}{WrapValue(queryColumn.Query.QueryAlias)}";

                var subCtx = CompileSelectQuery(queryColumn.Query);

                ctx.Bindings.AddRange(subCtx.Bindings);

                return "(" + subCtx.RawSql + $"){alias}";
            }

            if (column is AggregatedColumn aggregatedColumn)
            {
                var agg = aggregatedColumn.Aggregate.ToUpperInvariant();

                var (col, alias) = SplitAlias(CompileColumn(ctx, aggregatedColumn.Column));

                alias = string.IsNullOrEmpty(alias) ? "" : $" {ColumnAsKeyword}{alias}";

                var filterCondition = CompileFilterConditions(ctx, aggregatedColumn);

                if (string.IsNullOrEmpty(filterCondition)) return $"{agg}({col}){alias}";

                if (SupportsFilterClause) return $"{agg}({col}) FILTER (WHERE {filterCondition}){alias}";

                return $"{agg}(CASE WHEN {filterCondition} THEN {col} END){alias}";
            }

            return Wrap(((Column)column).Name);
        }

        private string? CompileFilterConditions(SqlResult ctx, AggregatedColumn aggregatedColumn)
        {
            if (aggregatedColumn.Filter == null) return null;

            var wheres = aggregatedColumn.Filter.GetComponents<AbstractCondition>("where");

            return CompileConditions(ctx, wheres);
        }

        public SqlResult CompileCte(AbstractFrom? cte)
        {
            var ctx = new SqlResult { Query = null };

            if (cte == null) return ctx;

            if (cte is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                Debug.Assert(raw.Alias != null, "raw.Alias != null");
                ctx.RawSql = $"{WrapValue(raw.Alias)} AS ({WrapIdentifiers(raw.Expression)})";
            }
            else if (cte is QueryFromClause queryFromClause)
            {
                var subCtx = CompileSelectQuery(queryFromClause.Query);
                ctx.Bindings.AddRange(subCtx.Bindings);

                Debug.Assert(queryFromClause.Alias != null, "queryFromClause.Alias != null");
                ctx.RawSql = $"{WrapValue(queryFromClause.Alias)} AS ({subCtx.RawSql})";
            }
            else if (cte is AdHocTableFromClause adHoc)
            {
                var subCtx = CompileAdHocQuery(adHoc);
                ctx.Bindings.AddRange(subCtx.Bindings);

                Debug.Assert(adHoc.Alias != null, "adHoc.Alias != null");
                ctx.RawSql = $"{WrapValue(adHoc.Alias)} AS ({subCtx.RawSql})";
            }

            return ctx;
        }

        protected SqlResult OnBeforeSelect(SqlResult ctx)
        {
            return ctx;
        }

        protected virtual string CompileColumns(SqlResult ctx)
        {
            if (ctx.Query.HasComponent("aggregate", EngineCode))
            {
                var aggregate = ctx.Query.GetOneComponent<AggregateClause>("aggregate", EngineCode);
                Debug.Assert(aggregate != null);

                var aggregateColumns = aggregate.Columns
                    .Select(Wrap)
                    .ToList();

                if (aggregateColumns.Count == 1)
                {
                    var sql = string.Join(", ", aggregateColumns);

                    if (ctx.Query.IsDistinct) sql = "DISTINCT " + sql;

                    return "SELECT " + aggregate.Type.ToUpperInvariant() + "(" + sql + $") {ColumnAsKeyword}" +
                           WrapValue(aggregate.Type);
                }

                return "SELECT 1";
            }

            var columns = ctx.Query
                .GetComponents<AbstractColumn>("select", EngineCode)
                .Select(x => CompileColumn(ctx, x))
                .ToList();

            var distinct = ctx.Query.IsDistinct ? "DISTINCT " : "";

            var select = columns.Any() ? string.Join(", ", columns) : "*";

            return $"SELECT {distinct}{select}";
        }

        public string? CompileUnion(SqlResult ctx)
        {
            // Handle UNION, EXCEPT and INTERSECT
            if (!ctx.Query.GetComponents("combine", EngineCode).Any()) return null;

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

                    combinedQueries.Add(WrapIdentifiers(combineRawClause.Expression));
                }

            return string.Join(" ", combinedQueries);
        }

        public string CompileTableExpression(SqlResult ctx, AbstractFrom from)
        {
            if (from is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;

                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias)
                    ? ""
                    : $" {TableAsKeyword}" + WrapValue(fromQuery.QueryAlias);

                var subCtx = CompileSelectQuery(fromQuery);

                ctx.Bindings.AddRange(subCtx.Bindings);

                return "(" + subCtx.RawSql + ")" + alias;
            }

            if (from is FromClause fromClause) return Wrap(fromClause.Table);

            throw InvalidClauseException("TableExpression", from);
        }

        public string CompileFrom(SqlResult ctx)
        {
            if (ctx.Query.HasComponent("from", EngineCode))
            {
                var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

                Debug.Assert(from != null, nameof(from) + " != null");
                return "FROM " + CompileTableExpression(ctx, from);
            }

            return string.Empty;
        }

        public string? CompileJoins(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("join", EngineCode)) return null;

            var joins = ctx.Query
                .GetComponents<BaseJoin>("join", EngineCode)
                .Select(x => CompileJoin(ctx, x.Join));

            return "\n" + string.Join("\n", joins);
        }

        public string CompileJoin(SqlResult ctx, Join join)
        {
            var from = join.BaseQuery.GetOneComponent<AbstractFrom>("from", EngineCode);
            var conditions = join.BaseQuery.GetComponents<AbstractCondition>("where", EngineCode);

            Debug.Assert(from != null, nameof(from) + " != null");
            var joinTable = CompileTableExpression(ctx, from);
            var constraints = CompileConditions(ctx, conditions);

            var onClause = conditions.Any() ? $" ON {constraints}" : "";

            return $"{join.Type} {joinTable}{onClause}";
        }

        public string? CompileWheres(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("where", EngineCode)) return null;

            var conditions = ctx.Query.GetComponents<AbstractCondition>("where", EngineCode);
            var sql = CompileConditions(ctx, conditions).Trim();

            return string.IsNullOrEmpty(sql) ? null : $"WHERE {sql}";
        }

        public string? CompileGroups(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("group", EngineCode)) return null;

            var columns = ctx.Query
                .GetComponents<AbstractColumn>("group", EngineCode)
                .Select(x => CompileColumn(ctx, x));

            return "GROUP BY " + string.Join(", ", columns);
        }

        public string? CompileOrders(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("order", EngineCode)) return null;

            var columns = ctx.Query
                .GetComponents<AbstractOrderBy>("order", EngineCode)
                .Select(x =>
                {
                    if (x is RawOrderBy raw)
                    {
                        ctx.Bindings.AddRange(raw.Bindings);
                        return WrapIdentifiers(raw.Expression);
                    }

                    var direction = ((OrderBy)x).Ascending ? "" : " DESC";

                    return Wrap(((OrderBy)x).Column) + direction;
                });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public string? CompileHaving(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("having", EngineCode)) return null;

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

            return $"HAVING {string.Join(" ", sql)}";
        }

        public virtual string? CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return null;

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return $"LIMIT {ParameterPlaceholder}";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"OFFSET {ParameterPlaceholder}";
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return $"LIMIT {ParameterPlaceholder} OFFSET {ParameterPlaceholder}";
        }

        /// <summary>
        ///     Compile the random statement into SQL.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public virtual string CompileRandom(string seed)
        {
            return "RANDOM()";
        }

        public string CompileLower(string value)
        {
            return $"LOWER({value})";
        }

        public string CompileUpper(string value)
        {
            return $"UPPER({value})";
        }

        public virtual string CompileTrue()
        {
            return "true";
        }

        public virtual string CompileFalse()
        {
            return "false";
        }

        private InvalidCastException InvalidClauseException(string section, AbstractClause clause)
        {
            return new InvalidCastException(
                $"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");
        }

        protected string CheckOperator(string op)
        {
            op = op.ToLowerInvariant();

            var valid = _operators.Contains(op) || _userOperators.Contains(op);

            if (!valid)
                throw new InvalidOperationException(
                    $"The operator '{op}' cannot be used. Please consider white listing it before using it.");

            return op;
        }

        /// <summary>
        ///     Wrap a single string in a column identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Wrap(string value)
        {
            var segments = value.Split(" as ");
            if (segments.Length> 1)
                return $"{Wrap(segments[0])} {ColumnAsKeyword}{WrapValue(segments[1])}";

            if (value.Contains("."))
                return string.Join(".", value.Split('.').Select((x, _) => WrapValue(x)));

            // If we reach here then the value does not contain an "AS" alias
            // nor dot "." expression, so wrap it as regular value.
            return WrapValue(value);
        }

        public (string, string?) SplitAlias(string value)
        {
            var index = value.LastIndexOf(" as ", StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                var before = value.Substring(0, index);
                var after = value.Substring(index + 4);
                return (before, after);
            }

            return (value, null);
        }

        /// <summary>
        ///     Wrap a single string in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string WrapValue(string value)
        {
            return value.Brace(OpeningIdentifier, ClosingIdentifier);
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
                return ParameterPlaceholder;
            }

            ctx.Bindings.Add(parameter);
            return ParameterPlaceholder;
        }

        /// <summary>
        ///     Create query parameter place-holders for an array.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public string Parametrize(SqlResult ctx, IEnumerable<object> values)
        {
            return string.Join(", ", values.Select(x => Parameter(ctx, x)));
        }

        public string WrapIdentifiers(string input)
        {
            return input

                // deprecated
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "{", OpeningIdentifier)
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "}", ClosingIdentifier)
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "[", OpeningIdentifier)
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "]", ClosingIdentifier);
        }
    }
}
