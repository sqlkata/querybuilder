using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private readonly ConditionsCompilerProvider _compileConditionMethodsProvider;
        protected virtual string parameterPlaceholder { get; set; } = "?";
        protected virtual string parameterPrefix { get; set; } = "@p";
        protected virtual string OpeningIdentifier { get; set; } = "\"";
        protected virtual string ClosingIdentifier { get; set; } = "\"";
        protected virtual string ColumnAsKeyword { get; set; } = "AS ";
        protected virtual string TableAsKeyword { get; set; } = "AS ";
        protected virtual string LastId { get; set; } = "";
        protected virtual string EscapeCharacter { get; set; } = "\\";


        protected virtual string SingleInsertStartClause { get; set; } = "INSERT INTO";
        protected virtual string MultiInsertStartClause { get; set; } = "INSERT INTO";


        protected Compiler()
        {
            _compileConditionMethodsProvider = new ConditionsCompilerProvider(this);
        }

        public virtual string EngineCode { get; }

        /// <summary>
        /// Whether the compiler supports the `SELECT ... FILTER` syntax
        /// </summary>
        /// <value></value>            
        public virtual bool SupportsFilterClause { get; set; } = false;

        /// <summary>
        /// If true the compiler will remove the SELECT clause for the query used inside WHERE EXISTS
        /// </summary>
        /// <value></value>            
        public virtual bool OmitSelectInsideExists { get; set; } = true;

        protected virtual string SingleRowDummyTableName { get => null; }

        /// <summary>
        /// A list of white-listed operators
        /// </summary>
        /// <value></value>
        protected readonly HashSet<string> operators = new HashSet<string>
        {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "not like",
            "ilike", "not ilike",
            "like binary", "not like binary",
            "rlike", "not rlike",
            "regexp", "not regexp",
            "similar to", "not similar to"
        };

        protected HashSet<string> userOperators = new HashSet<string>
        {

        };

        protected Dictionary<string, object> generateNamedBindings(object[] bindings)
        {
            return Helper.Flatten(bindings).Select((v, i) => new { i, v })
                .ToDictionary(x => parameterPrefix + x.i, x => x.v);
        }

        protected SqlResult PrepareResult(SqlResult ctx)
        {
            ctx.NamedBindings = generateNamedBindings(ctx.Bindings.ToArray());
            ctx.Sql = Helper.ReplaceAll(ctx.RawSql, parameterPlaceholder, i => parameterPrefix + i);
            return ctx;
        }


        private Query TransformAggregateQuery(Query query)
        {
            var clause = query.GetOneComponent<AggregateClause>("aggregate", EngineCode);

            if (clause.Columns.Count == 1 && !query.IsDistinct) return query;

            if (query.IsDistinct)
            {
                query.ClearComponent("aggregate", EngineCode);
                query.ClearComponent("select", EngineCode);
                query.Select(clause.Columns.ToArray());
            }
            else
            {
                foreach (var column in clause.Columns)
                {
                    query.WhereNotNull(column);
                }
            }

            var outerClause = new AggregateClause()
            {
                Columns = new List<string> { "*" },
                Type = clause.Type
            };

            return new Query()
                .AddComponent("aggregate", outerClause)
                .From(query, $"{clause.Type}Query");
        }

        protected virtual SqlResult CompileRaw(Query query)
        {
            SqlResult ctx;

            if (query.Method == "insert")
            {
                ctx = CompileInsertQuery(query);
            }
            else if (query.Method == "update")
            {
                ctx = CompileUpdateQuery(query);
            }
            else if (query.Method == "delete")
            {
                ctx = CompileDeleteQuery(query);
            }
            else
            {
                if (query.Method == "aggregate")
                {
                    query.ClearComponent("limit")
                        .ClearComponent("order")
                        .ClearComponent("group");

                    query = TransformAggregateQuery(query);
                }

                ctx = CompileSelectQuery(query);
            }

            // handle CTEs
            if (query.HasComponent("cte", EngineCode))
            {
                ctx = CompileCteQuery(ctx, query);
            }

            ctx.RawSql = Helper.ExpandParameters(ctx.RawSql, parameterPlaceholder, ctx.Bindings.ToArray());

            return ctx;
        }

        /// <summary>
        /// Add the passed operator(s) to the white list so they can be used with
        /// the Where/Having methods, this prevent passing arbitrary operators
        /// that opens the door for SQL injections.
        /// </summary>
        /// <param name="operators"></param>
        /// <returns></returns>
        public Compiler Whitelist(params string[] operators)
        {
            foreach (var op in operators)
            {
                this.userOperators.Add(op);
            }

            return this;
        }

        public virtual SqlResult Compile(Query query)
        {
            var ctx = CompileRaw(query);

            ctx = PrepareResult(ctx);

            return ctx;
        }

        public virtual SqlResult Compile(IEnumerable<Query> queries)
        {
            var compiled = queries.Select(CompileRaw).ToArray();
            var bindings = compiled.Select(r => r.Bindings).ToArray();
            var totalBindingsCount = bindings.Select(b => b.Count).Aggregate((a, b) => a + b);

            var combinedBindings = new List<object>(totalBindingsCount);
            foreach (var cb in bindings)
            {
                combinedBindings.AddRange(cb);
            }

            var ctx = new SqlResult
            {
                RawSql = compiled.Select(r => r.RawSql).Aggregate((a, b) => a + ";\n" + b),
                Bindings = combinedBindings,
            };

            ctx = PrepareResult(ctx);

            return ctx;
        }

        protected virtual SqlResult CompileSelectQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query.Clone(),
            };

            var results = new[] {
                    this.CompileColumns(ctx),
                    this.CompileFrom(ctx),
                    this.CompileJoins(ctx),
                    this.CompileWheres(ctx),
                    this.CompileGroups(ctx),
                    this.CompileHaving(ctx),
                    this.CompileOrders(ctx),
                    this.CompileLimit(ctx),
                    this.CompileUnion(ctx),
                }
               .Where(x => x != null)
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            string sql = string.Join(" ", results);

            ctx.RawSql = sql;

            return ctx;
        }

        protected virtual SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
        {
            var ctx = new SqlResult();

            var firstRow = "SELECT " + string.Join(", ", adHoc.Columns.Select(col => $"{parameterPlaceholder} {ColumnAsKeyword}{Wrap(col)}"));
            var row = "SELECT " + string.Join(", ", adHoc.Columns.Select(col => $"{parameterPlaceholder}"));

            var fromTable = SingleRowDummyTableName;

            if (fromTable != null)
            {
                var fromClause = $" FROM {fromTable}";
                firstRow += fromClause;
                row += fromClause;
            }

            var rowCount = adHoc.Values.Count / adHoc.Columns.Count;
            var rows = rowCount > 0
                ? string.Join(" UNION ALL ", Enumerable.Repeat(row, rowCount - 1).Prepend(firstRow))
                : string.Empty;

            ctx.RawSql = rows;
            ctx.Bindings = adHoc.Values;

            return ctx;
        }

        protected virtual SqlResult CompileDeleteQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to delete");
            }

            var fromClause = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            string table = null;

            if (fromClause is FromClause fromClauseCast)
            {
                table = Wrap(fromClauseCast.Table);
            }

            if (fromClause is RawFromClause rawFromClause)
            {
                table = WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null)
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            var joins = CompileJoins(ctx);

            var where = CompileWheres(ctx);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            if (string.IsNullOrEmpty(joins))
            {
                ctx.RawSql = $"DELETE FROM {table}{where}";
            }
            else
            {
                // check if we have alias 
                if (fromClause is FromClause && !string.IsNullOrEmpty(fromClause.Alias))
                {
                    ctx.RawSql = $"DELETE {Wrap(fromClause.Alias)} FROM {table} {joins}{where}";
                }
                else
                {
                    ctx.RawSql = $"DELETE {table} FROM {table} {joins}{where}";
                }

            }

            return ctx;
        }

        protected virtual SqlResult CompileUpdateQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to update");
            }

            var fromClause = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            string table = null;

            if (fromClause is FromClause fromClauseCast)
            {
                table = Wrap(fromClauseCast.Table);
            }

            if (fromClause is RawFromClause rawFromClause)
            {
                table = WrapIdentifiers(rawFromClause.Expression);
                ctx.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null)
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            // check for increment statements
            var clause = ctx.Query.GetOneComponent("update", EngineCode);

            string wheres;

            if (clause != null && clause is IncrementClause increment)
            {
                var column = Wrap(increment.Column);
                var value = Parameter(ctx, Math.Abs(increment.Value));
                var sign = increment.Value >= 0 ? "+" : "-";

                wheres = CompileWheres(ctx);

                if (!string.IsNullOrEmpty(wheres))
                {
                    wheres = " " + wheres;
                }

                ctx.RawSql = $"UPDATE {table} SET {column} = {column} {sign} {value}{wheres}";

                return ctx;
            }


            var toUpdate = ctx.Query.GetOneComponent<InsertClause>("update", EngineCode);
            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Count; i++)
            {
                parts.Add(Wrap(toUpdate.Columns[i]) + " = " + Parameter(ctx, toUpdate.Values[i]));
            }

            var sets = string.Join(", ", parts);

            wheres = CompileWheres(ctx);

            if (!string.IsNullOrEmpty(wheres))
            {
                wheres = " " + wheres;
            }

            ctx.RawSql = $"UPDATE {table} SET {sets}{wheres}";

            return ctx;
        }

        protected virtual SqlResult CompileInsertQuery(Query query)
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

            string table = null;
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
            else
                return CompileValueInsertClauses(ctx, table, inserts.Cast<InsertClause>());
        }

        protected virtual SqlResult CompileInsertQueryClause(
            SqlResult ctx, string table, InsertQueryClause clause)
        {
            string columns = GetInsertColumnsList(clause.Columns);

            var subCtx = CompileSelectQuery(clause.Query);
            ctx.Bindings.AddRange(subCtx.Bindings);

            ctx.RawSql = $"{SingleInsertStartClause} {table}{columns} {subCtx.RawSql}";

            return ctx;
        }

        protected virtual SqlResult CompileValueInsertClauses(
            SqlResult ctx, string table, IEnumerable<InsertClause> insertClauses)
        {
            bool isMultiValueInsert = insertClauses.Skip(1).Any();

            var insertInto = (isMultiValueInsert) ? MultiInsertStartClause : SingleInsertStartClause;

            var firstInsert = insertClauses.First();
            string columns = GetInsertColumnsList(firstInsert.Columns);
            var values = string.Join(", ", Parameterize(ctx, firstInsert.Values));

            ctx.RawSql = $"{insertInto} {table}{columns} VALUES ({values})";

            if (isMultiValueInsert)
                return CompileRemainingInsertClauses(ctx, table, insertClauses);

            if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
                ctx.RawSql += ";" + LastId;

            return ctx;
        }

        protected virtual SqlResult CompileRemainingInsertClauses(SqlResult ctx, string table, IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                string values = string.Join(", ", Parameterize(ctx, insert.Values));
                ctx.RawSql += $", ({values})";
            }
            return ctx;
        }

        protected string GetInsertColumnsList(List<string> columnList)
        {
            var columns = "";
            if (columnList.Any())
                columns = $" ({string.Join(", ", WrapArray(columnList))})";

            return columns;
        }

        protected virtual SqlResult CompileCteQuery(SqlResult ctx, Query query)
        {
            var cteFinder = new CteFinder(query, EngineCode);
            var cteSearchResult = cteFinder.Find();

            var rawSql = new StringBuilder("WITH ");
            var cteBindings = new List<object>();

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
        /// Compile a single column clause
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string CompileColumn(SqlResult ctx, AbstractColumn column)
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
                {
                    alias = $" {ColumnAsKeyword}{WrapValue(queryColumn.Query.QueryAlias)}";
                }

                var subCtx = CompileSelectQuery(queryColumn.Query);

                ctx.Bindings.AddRange(subCtx.Bindings);

                return "(" + subCtx.RawSql + $"){alias}";
            }

            if (column is AggregatedColumn aggregatedColumn)
            {
                string agg = aggregatedColumn.Aggregate.ToUpperInvariant();

                var (col, alias) = SplitAlias(CompileColumn(ctx, aggregatedColumn.Column));

                alias = string.IsNullOrEmpty(alias) ? "" : $" {ColumnAsKeyword}{alias}";

                string filterCondition = CompileFilterConditions(ctx, aggregatedColumn);

                if (string.IsNullOrEmpty(filterCondition))
                {
                    return $"{agg}({col}){alias}";
                }

                if (SupportsFilterClause)
                {
                    return $"{agg}({col}) FILTER (WHERE {filterCondition}){alias}";
                }

                return $"{agg}(CASE WHEN {filterCondition} THEN {col} END){alias}";
            }

            return Wrap(((Column)column).Name);

        }

        protected virtual string CompileFilterConditions(SqlResult ctx, AggregatedColumn aggregatedColumn)
        {
            if (aggregatedColumn.Filter == null)
            {
                return null;
            }

            var wheres = aggregatedColumn.Filter.GetComponents<AbstractCondition>("where");

            return CompileConditions(ctx, wheres);
        }

        public virtual SqlResult CompileCte(AbstractFrom cte)
        {
            var ctx = new SqlResult();

            if (null == cte)
            {
                return ctx;
            }

            if (cte is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                ctx.RawSql = $"{WrapValue(raw.Alias)} AS ({WrapIdentifiers(raw.Expression)})";
            }
            else if (cte is QueryFromClause queryFromClause)
            {
                var subCtx = CompileSelectQuery(queryFromClause.Query);
                ctx.Bindings.AddRange(subCtx.Bindings);

                ctx.RawSql = $"{WrapValue(queryFromClause.Alias)} AS ({subCtx.RawSql})";
            }
            else if (cte is AdHocTableFromClause adHoc)
            {
                var subCtx = CompileAdHocQuery(adHoc);
                ctx.Bindings.AddRange(subCtx.Bindings);

                ctx.RawSql = $"{WrapValue(adHoc.Alias)} AS ({subCtx.RawSql})";
            }

            return ctx;
        }

        protected virtual SqlResult OnBeforeSelect(SqlResult ctx)
        {
            return ctx;
        }

        protected virtual string CompileColumns(SqlResult ctx)
        {
            if (ctx.Query.HasComponent("aggregate", EngineCode))
            {
                var aggregate = ctx.Query.GetOneComponent<AggregateClause>("aggregate", EngineCode);

                var aggregateColumns = aggregate.Columns
                    .Select(x => CompileColumn(ctx, new Column { Name = x }))
                    .ToList();

                if (aggregateColumns.Count == 1)
                {
                    var sql = string.Join(", ", aggregateColumns);

                    if (ctx.Query.IsDistinct)
                    {
                        sql = "DISTINCT " + sql;
                    }

                    return "SELECT " + aggregate.Type.ToUpperInvariant() + "(" + sql + $") {ColumnAsKeyword}" + WrapValue(aggregate.Type);
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

        public virtual string CompileUnion(SqlResult ctx)
        {

            // Handle UNION, EXCEPT and INTERSECT
            if (!ctx.Query.GetComponents("combine", EngineCode).Any())
            {
                return null;
            }

            var combinedQueries = new List<string>();

            var clauses = ctx.Query.GetComponents<AbstractCombine>("combine", EngineCode);

            foreach (var clause in clauses)
            {
                if (clause is Combine combineClause)
                {
                    var combineOperator = combineClause.Operation.ToUpperInvariant() + " " + (combineClause.All ? "ALL " : "");

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
            }

            return string.Join(" ", combinedQueries);

        }

        public virtual string CompileTableExpression(SqlResult ctx, AbstractFrom from)
        {
            if (from is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;

                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : $" {TableAsKeyword}" + WrapValue(fromQuery.QueryAlias);

                var subCtx = CompileSelectQuery(fromQuery);

                ctx.Bindings.AddRange(subCtx.Bindings);

                return "(" + subCtx.RawSql + ")" + alias;
            }

            if (from is FromClause fromClause)
            {
                return Wrap(fromClause.Table);
            }

            throw InvalidClauseException("TableExpression", from);
        }

        public virtual string CompileFrom(SqlResult ctx)
        {
            if (ctx.Query.HasComponent("from", EngineCode))
            {
                var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

                return "FROM " + CompileTableExpression(ctx, from);
            }

            return string.Empty;
        }

        public virtual string CompileJoins(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("join", EngineCode))
            {
                return null;
            }

            var joins = ctx.Query
                .GetComponents<BaseJoin>("join", EngineCode)
                .Select(x => CompileJoin(ctx, x.Join));

            return "\n" + string.Join("\n", joins);
        }

        public virtual string CompileJoin(SqlResult ctx, Join join, bool isNested = false)
        {

            var from = join.GetOneComponent<AbstractFrom>("from", EngineCode);
            var conditions = join.GetComponents<AbstractCondition>("where", EngineCode);

            var joinTable = CompileTableExpression(ctx, from);
            var constraints = CompileConditions(ctx, conditions);

            var onClause = conditions.Any() ? $" ON {constraints}" : "";

            return $"{join.Type} {joinTable}{onClause}";
        }

        public virtual string CompileWheres(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("where", EngineCode))
            {
                return null;
            }

            var conditions = ctx.Query.GetComponents<AbstractCondition>("where", EngineCode);
            var sql = CompileConditions(ctx, conditions).Trim();

            return string.IsNullOrEmpty(sql) ? null : $"WHERE {sql}";
        }

        public virtual string CompileGroups(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("group", EngineCode))
            {
                return null;
            }

            var columns = ctx.Query
                .GetComponents<AbstractColumn>("group", EngineCode)
                .Select(x => CompileColumn(ctx, x));

            return "GROUP BY " + string.Join(", ", columns);
        }

        public virtual string CompileOrders(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("order", EngineCode))
            {
                return null;
            }

            var columns = ctx.Query
                .GetComponents<AbstractOrderBy>("order", EngineCode)
                .Select(x =>
            {

                if (x is RawOrderBy raw)
                {
                    ctx.Bindings.AddRange(raw.Bindings);
                    return WrapIdentifiers(raw.Expression);
                }

                var orderByClause = (OrderBy)x;
                var direction = orderByClause.Ascending ? "" : " DESC";

                return Wrap(orderByClause.Column) + direction;
            });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public virtual string CompileHaving(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("having", EngineCode))
            {
                return null;
            }

            var sql = new List<string>();
            string boolOperator;

            var having = ctx.Query.GetComponents("having", EngineCode)
                .Cast<AbstractCondition>()
                .ToList();

            for (var i = 0; i < having.Count; i++)
            {
                var compiled = CompileCondition(ctx, having[i]);

                if (!string.IsNullOrEmpty(compiled))
                {
                    boolOperator = i > 0 ? having[i].IsOr ? "OR " : "AND " : "";

                    sql.Add(boolOperator + compiled);
                }
            }

            return $"HAVING {string.Join(" ", sql)}";
        }

        public virtual string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return $"LIMIT {parameterPlaceholder}";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"OFFSET {parameterPlaceholder}";
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return $"LIMIT {parameterPlaceholder} OFFSET {parameterPlaceholder}";
        }

        /// <summary>
        /// Compile the random statement into SQL.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public virtual string CompileRandom(string seed)
        {
            return "RANDOM()";
        }

        public virtual string CompileLower(string value)
        {
            return $"LOWER({value})";
        }

        public virtual string CompileUpper(string value)
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
            return new InvalidCastException($"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");
        }

        protected string checkOperator(string op)
        {
            op = op.ToLowerInvariant();

            var valid = operators.Contains(op) || userOperators.Contains(op);

            if (!valid)
            {
                throw new InvalidOperationException($"The operator '{op}' cannot be used. Please consider white listing it before using it.");
            }

            return op;
        }

        /// <summary>
        /// Wrap a single string in a column identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string Wrap(string value)
        {

            if (value.ToLowerInvariant().Contains(" as "))
            {
                var (before, after) = SplitAlias(value);

                return Wrap(before) + $" {ColumnAsKeyword}" + WrapValue(after);
            }

            if (value.Contains("."))
            {
                return string.Join(".", value.Split('.').Select((x, index) =>
                {
                    return WrapValue(x);
                }));
            }

            // If we reach here then the value does not contain an "AS" alias
            // nor dot "." expression, so wrap it as regular value.
            return WrapValue(value);
        }

        public virtual (string, string) SplitAlias(string value)
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
        /// Wrap a single string in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string WrapValue(string value)
        {
            if (value == "*") return value;

            var opening = this.OpeningIdentifier;
            var closing = this.ClosingIdentifier;

            if (string.IsNullOrWhiteSpace(opening) && string.IsNullOrWhiteSpace(closing)) return value;

            return opening + value.Replace(closing, closing + closing) + closing;
        }

        /// <summary>
        /// Resolve a parameter
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public virtual object Resolve(SqlResult ctx, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal)
            {
                return literal.Value;
            }

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
            {
                var value = ctx.Query.FindVariable(variable.Name);
                return value;
            }

            return parameter;

        }

        /// <summary>
        /// Resolve a parameter and add it to the binding list
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public virtual string Parameter(SqlResult ctx, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal)
            {
                return literal.Value;
            }

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
            {
                var value = ctx.Query.FindVariable(variable.Name);
                ctx.Bindings.Add(value);
                return parameterPlaceholder;
            }

            ctx.Bindings.Add(parameter);
            return parameterPlaceholder;
        }

        /// <summary>
        /// Create query parameter place-holders for an array.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual string Parameterize<T>(SqlResult ctx, IEnumerable<T> values)
        {
            return string.Join(", ", values.Select(x => Parameter(ctx, x)));
        }

        /// <summary>
        /// Wrap an array of values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual List<string> WrapArray(List<string> values)
        {
            return values.Select(x => Wrap(x)).ToList();
        }

        public virtual string WrapIdentifiers(string input)
        {
            return input

                // deprecated
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "{", this.OpeningIdentifier)
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "}", this.ClosingIdentifier)

                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "[", this.OpeningIdentifier)
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "]", this.ClosingIdentifier);
        }
    }
}
