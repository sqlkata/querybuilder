using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlKata.Compilers
{
    public partial class Compiler
    {
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
        }

        public virtual string EngineCode { get; }

        /// <summary>
        /// Whether the compiler supports the `SELECT ... FILTER` syntax
        /// </summary>
        public virtual bool SupportsFilterClause { get; set; } = false;

        /// <summary>
        /// If true the compiler will remove the SELECT clause for the query used inside WHERE EXISTS
        /// </summary>
        public virtual bool OmitSelectInsideExists { get; set; } = true;

        protected virtual string SingleRowDummyTableName => null;

        /// <summary>
        /// A list of white-listed operators
        /// </summary>
        protected readonly HashSet<string> operators =
        [
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "not like",
            "ilike", "not ilike",
            "like binary", "not like binary",
            "rlike", "not rlike",
            "regexp", "not regexp",
            "similar to", "not similar to"
        ];

        protected HashSet<string> userOperators = [];

        protected Dictionary<string, object> generateNamedBindings(object[] bindings)
        {
            return Helper.Flatten(bindings).Select((v, i) => new { i, v })
                .ToDictionary(x => parameterPrefix + x.i, x => x.v);
        }

        protected SqlResult PrepareResult(CompileContext ctx)
        {
            return new SqlResult(parameterPlaceholder, EscapeCharacter)
            {
                Query = ctx.Query,
                RawSql = ctx.RawSql,
                Bindings = ctx.Bindings,
                NamedBindings = generateNamedBindings(ctx.Bindings.ToArray()),
                Sql = Helper.ReplaceAll(ctx.RawSql, parameterPlaceholder, EscapeCharacter, i => parameterPrefix + i)
            };
        }

        private Query TransformAggregateQuery(Query query)
        {
            var clause = query.AggregateClauses.ForEngineOrDefault(EngineCode);

            // Case 1: Simple single-column aggregate without DISTINCT.
            if (clause.Columns.Count == 1 && !query.IsDistinct)
            {
                var single = query.Clone();
                single.LimitClauses.Clear();
                single.OrderByClauses.Clear();
                single.GroupByClauses.Clear();
                return single;
            }

            // Case 2 & 3: Multi-column aggregates or DISTINCT aggregates require wrapping the query in a subquery:
            var transformed = query.Clone();
            transformed.LimitClauses.Clear();
            transformed.OrderByClauses.Clear();
            transformed.GroupByClauses.Clear();

            if (transformed.IsDistinct)
            {
                transformed.AggregateClauses.ClearForEngine(EngineCode);
                transformed.SelectClauses.ClearForEngine(EngineCode);
                transformed.Select(clause.Columns.ToArray());
            }
            else
            {
                foreach (var column in clause.Columns)
                {
                    transformed.WhereNotNull(column);
                }
            }

            var outerClause = new AggregateClause()
            {
                Columns = new List<string> { "*" },
                Type = clause.Type,
                Engine = EngineCode
            };

            var outerQuery = new Query()
                .From(transformed, $"{clause.Type}Query");
            outerQuery.AggregateClauses.Add(outerClause);
            return outerQuery;
        }

        protected virtual CompileContext CompileRaw(Query query)
        {
            var ctx = new CompileContext(query, parameterPlaceholder, EscapeCharacter);
            return CompileRaw(ctx);
        }

        protected virtual CompileContext CompileRaw(CompileContext ctx)
        {
            VisitQuery(ctx, ctx.Query);
            ctx.RawSql = Helper.ExpandParameters(ctx.Buffer.ToString(), parameterPlaceholder, EscapeCharacter, ctx.Bindings.ToArray());
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

            return PrepareResult(ctx);
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

            var ctx = new CompileContext(null, parameterPlaceholder, EscapeCharacter)
            {
                RawSql = compiled.Select(r => r.RawSql).Aggregate((a, b) => a + ";\n" + b),
                Bindings = combinedBindings,
            };

            return PrepareResult(ctx);
        }

        public virtual void CompileSubQuery(CompileContext ctx, Query query)
        {
            ctx.PushQuery(query);
            try
            {
                VisitSelectQuery(ctx, query);
            }
            finally
            {
                ctx.PopQuery();
            }
        }

        // ==========================================
        // Visitor Methods: Queries / Statements
        // ==========================================

        public virtual void VisitQuery(CompileContext ctx, Query query)
        {
            if (query.CteClauses.ForEngine(EngineCode).Any())
            {
                VisitCteQuery(ctx, query);
            }

            if (query.Method == "insert")
            {
                VisitInsertQuery(ctx, query);
            }
            else if (query.Method == "update")
            {
                VisitUpdateQuery(ctx, query);
            }
            else if (query.Method == "delete")
            {
                VisitDeleteQuery(ctx, query);
            }
            else
            {
                if (query.Method == "aggregate")
                {
                    query = TransformAggregateQuery(query);
                    ctx.Query = query;
                }

                VisitSelectQuery(ctx, query);
            }
        }

        public virtual void VisitSelectQuery(CompileContext ctx, Query query)
        {
            VisitColumns(ctx, query);
            VisitFrom(ctx, query);
            VisitJoins(ctx, query);
            VisitWheres(ctx, query);
            VisitGroups(ctx, query);
            VisitHaving(ctx, query);
            VisitOrders(ctx, query);
            VisitLimit(ctx, query);
            VisitUnions(ctx, query);
        }

        public virtual void VisitDeleteQuery(CompileContext ctx, Query query)
        {
            var fromClause = query.FromClauses.ForEngineOrDefault(EngineCode);
            if (fromClause is null)
            {
                throw new InvalidOperationException("No table set to delete");
            }

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

            var hasJoins = query.Joins.ForEngine(EngineCode).Any();

            if (!hasJoins)
            {
                ctx.Write($"DELETE FROM {table}");
                VisitWheres(ctx, query);
                return;
            }

            if (fromClause is FromClause && !string.IsNullOrEmpty(fromClause.Alias))
            {
                ctx.Write($"DELETE {Wrap(fromClause.Alias)} FROM {table}");
            }
            else
            {
                ctx.Write($"DELETE {table} FROM {table}");
            }

            VisitJoins(ctx, query);
            VisitWheres(ctx, query);
        }

        public virtual void VisitUpdateQuery(CompileContext ctx, Query query)
        {
            var fromClause = query.FromClauses.ForEngineOrDefault(EngineCode);
            if (fromClause is null)
            {
                throw new InvalidOperationException("No table set to update");
            }

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

            var clause = query.UpdateClauses.ForEngineOrDefault(EngineCode);

            if (clause != null && clause is IncrementClause increment)
            {
                var column = Wrap(increment.Column);
                var value = Parameter(ctx, Math.Abs(increment.Value));
                var sign = increment.Value >= 0 ? "+" : "-";

                ctx.Write($"UPDATE {table} SET {column} = {column} {sign} {value}");
                VisitWheres(ctx, query);
                return;
            }

            var toUpdate = clause as InsertClause;
            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Count; i++)
            {
                parts.Add(Wrap(toUpdate.Columns[i]) + " = " + Parameter(ctx, toUpdate.Values[i]));
            }

            var sets = string.Join(", ", parts);

            ctx.Write($"UPDATE {table} SET {sets}");
            VisitWheres(ctx, query);
        }

        public virtual void VisitInsertQuery(CompileContext ctx, Query query)
        {
            var fromClause = query.FromClauses.ForEngineOrDefault(EngineCode);
            if (fromClause is null)
                throw new InvalidOperationException("No table set to insert");

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

            var inserts = query.InsertClauses.ForEngine(EngineCode).ToList();
            if (inserts[0] is InsertQueryClause insertQueryClause)
                VisitInsertQueryClause(ctx, table, insertQueryClause);
            else
                VisitValueInsertClauses(ctx, table, inserts.Cast<InsertClause>());
        }

        public virtual void VisitInsertQueryClause(
            CompileContext ctx, string table, InsertQueryClause clause)
        {
            string columns = GetInsertColumnsList(clause.Columns);

            ctx.Write($"{SingleInsertStartClause} {table}{columns} ");
            CompileSubQuery(ctx, clause.Query);
        }

        public virtual void VisitValueInsertClauses(
            CompileContext ctx, string table, IEnumerable<InsertClause> insertClauses)
        {
            var list = insertClauses.ToList();
            bool isMultiValueInsert = list.Skip(1).Any();

            var insertInto = isMultiValueInsert ? MultiInsertStartClause : SingleInsertStartClause;

            var firstInsert = list.First();
            string columns = GetInsertColumnsList(firstInsert.Columns);
            var values = string.Join(", ", Parameterize(ctx, firstInsert.Values));

            ctx.Write($"{insertInto} {table}{columns} VALUES ({values})");

            if (isMultiValueInsert)
            {
                VisitRemainingInsertClauses(ctx, table, list);
                return;
            }

            if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
            {
                ctx.Write(";" + LastId);
            }
        }

        public virtual void VisitRemainingInsertClauses(CompileContext ctx, string table, IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                string values = string.Join(", ", Parameterize(ctx, insert.Values));
                ctx.Write($", ({values})");
            }
        }

        public virtual void VisitCteQuery(CompileContext ctx, Query query)
        {
            var cteFinder = new CteFinder(query, EngineCode);
            var cteSearchResult = cteFinder.Find();

            ctx.Write("WITH ");

            for (var i = 0; i < cteSearchResult.Count; i++)
            {
                if (i > 0)
                {
                    ctx.Write(",\n");
                }
                VisitCte(ctx, cteSearchResult[i]);
            }

            ctx.Write("\n");
        }

        public virtual void VisitCte(CompileContext ctx, AbstractFrom cte)
        {
            if (cte == null) return;

            if (cte is RawFromClause raw)
            {
                ctx.Bindings.AddRange(raw.Bindings);
                ctx.Write($"{WrapValue(raw.Alias)} AS ({WrapIdentifiers(raw.Expression)})");
            }
            else if (cte is QueryFromClause queryFromClause)
            {
                ctx.Write($"{WrapValue(queryFromClause.Alias)} AS (");
                CompileSubQuery(ctx, queryFromClause.Query);
                ctx.Write(")");
            }
            else if (cte is AdHocTableFromClause adHoc)
            {
                ctx.Write($"{WrapValue(adHoc.Alias)} AS (");
                VisitAdHocTableFromClause(ctx, adHoc);
                ctx.Write(")");
            }
        }

        // ==========================================
        // Visitor Methods: Select Clauses & Elements
        // ==========================================

        public virtual void VisitColumns(CompileContext ctx, Query query)
        {
            VisitSelectPrefix(ctx, query);
            VisitColumnExpressions(ctx, query);
        }

        public virtual void VisitSelectPrefix(CompileContext ctx, Query query)
        {
            var aggregate = query.AggregateClauses.ForEngineOrDefault(EngineCode);
            if (aggregate != null)
            {
                ctx.Write("SELECT ");
                return;
            }

            var distinct = query.IsDistinct ? "DISTINCT " : "";
            ctx.Write($"SELECT {distinct}");
        }

        public virtual void VisitColumnExpressions(CompileContext ctx, Query query)
        {
            var aggregate = query.AggregateClauses.ForEngineOrDefault(EngineCode);
            if (aggregate != null)
            {
                if (aggregate.Columns.Count == 1)
                {
                    ctx.Write(aggregate.Type.ToUpperInvariant()).Write("(");
                    if (query.IsDistinct)
                    {
                        ctx.Write("DISTINCT ");
                    }
                    VisitColumn(ctx, new Column { Name = aggregate.Columns[0] });
                    ctx.Write($") {ColumnAsKeyword}").Write(WrapValue(aggregate.Type));
                    return;
                }

                ctx.Write("1");
                return;
            }

            var selectClauses = query.SelectClauses.ForEngine(EngineCode).ToList();
            if (!selectClauses.Any())
            {
                ctx.Write("*");
                return;
            }

            for (var i = 0; i < selectClauses.Count; i++)
            {
                if (i > 0) ctx.Write(", ");
                VisitColumn(ctx, selectClauses[i]);
            }
        }

        public virtual void VisitColumn(CompileContext ctx, AbstractColumn column)
        {
            switch (column)
            {
                case RawColumn raw:
                    VisitRawColumn(ctx, raw);
                    break;
                case QueryColumn queryColumn:
                    VisitQueryColumn(ctx, queryColumn);
                    break;
                case AggregatedColumn aggregatedColumn:
                    VisitAggregatedColumn(ctx, aggregatedColumn);
                    break;
                case Column col:
                    VisitBasicColumn(ctx, col);
                    break;
                default:
                    throw InvalidClauseException("Column", column);
            }
        }

        public virtual void VisitBasicColumn(CompileContext ctx, Column column)
        {
            ctx.Write(Wrap(column.Name));
        }

        public virtual void VisitRawColumn(CompileContext ctx, RawColumn raw)
        {
            ctx.Bindings.AddRange(raw.Bindings);
            ctx.Write(WrapIdentifiers(raw.Expression));
        }

        public virtual void VisitQueryColumn(CompileContext ctx, QueryColumn queryColumn)
        {
            ctx.Write("(");
            CompileSubQuery(ctx, queryColumn.Query);
            ctx.Write(")");

            if (!string.IsNullOrWhiteSpace(queryColumn.Query.QueryAlias))
            {
                ctx.Write($" {ColumnAsKeyword}").Write(WrapValue(queryColumn.Query.QueryAlias));
            }
        }

        public virtual void VisitAggregatedColumn(CompileContext ctx, AggregatedColumn aggregatedColumn)
        {
            string agg = aggregatedColumn.Aggregate.ToUpperInvariant();

            var tempCtx = new CompileContext(ctx);
            VisitColumn(tempCtx, aggregatedColumn.Column);
            ctx.Bindings.AddRange(tempCtx.Bindings);

            var (col, alias) = SplitAlias(tempCtx.Buffer.ToString());
            alias = string.IsNullOrEmpty(alias) ? "" : $" {ColumnAsKeyword}{alias}";

            var filterCtx = new CompileContext(ctx);
            VisitFilterConditions(filterCtx, aggregatedColumn);

            if (filterCtx.Buffer.Length == 0)
            {
                ctx.Write($"{agg}({col}){alias}");
                return;
            }

            ctx.Bindings.AddRange(filterCtx.Bindings);

            if (SupportsFilterClause)
            {
                ctx.Write($"{agg}({col}) FILTER (WHERE {filterCtx.Buffer}){alias}");
            }
            else
            {
                ctx.Write($"{agg}(CASE WHEN {filterCtx.Buffer} THEN {col} END){alias}");
            }
        }

        public virtual void VisitFilterConditions(CompileContext ctx, AggregatedColumn aggregatedColumn)
        {
            if (aggregatedColumn.Filter == null)
            {
                return;
            }

            var wheres = aggregatedColumn.Filter.Wheres.Clauses
                .ForEngine(EngineCode)
                .Cast<AbstractCondition>()
                .ToList();

            VisitConditions(ctx, wheres);
        }

        public virtual void VisitFrom(CompileContext ctx, Query query)
        {
            var from = query.FromClauses.ForEngineOrDefault(EngineCode);
            if (from != null)
            {
                ctx.EnsureSpace().Write("FROM ");
                VisitTableExpression(ctx, from);
            }
        }

        public virtual void VisitTableExpression(CompileContext ctx, AbstractFrom from)
        {
            switch (from)
            {
                case RawFromClause raw:
                    VisitRawFromClause(ctx, raw);
                    break;
                case QueryFromClause queryFromClause:
                    VisitQueryFromClause(ctx, queryFromClause);
                    break;
                case FromClause fromClause:
                    VisitFromClause(ctx, fromClause);
                    break;
                case AdHocTableFromClause adHoc:
                    VisitAdHocTableFromClause(ctx, adHoc);
                    break;
                default:
                    throw InvalidClauseException("TableExpression", from);
            }
        }

        public virtual void VisitFromClause(CompileContext ctx, FromClause fromClause)
        {
            ctx.Write(Wrap(fromClause.Table));
        }

        public virtual void VisitRawFromClause(CompileContext ctx, RawFromClause raw)
        {
            ctx.Bindings.AddRange(raw.Bindings);
            ctx.Write(WrapIdentifiers(raw.Expression));
        }

        public virtual void VisitQueryFromClause(CompileContext ctx, QueryFromClause queryFromClause)
        {
            var fromQuery = queryFromClause.Query;
            var alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : $" {TableAsKeyword}" + WrapValue(fromQuery.QueryAlias);
            ctx.Write("(");
            CompileSubQuery(ctx, fromQuery);
            ctx.Write(")").Write(alias);
        }

        public virtual void VisitAdHocTableFromClause(CompileContext ctx, AdHocTableFromClause adHoc)
        {
            var row = "SELECT " + string.Join(", ", adHoc.Columns.Select(col => $"{parameterPlaceholder} AS {Wrap(col)}"));

            var fromTable = SingleRowDummyTableName;
            if (fromTable != null)
            {
                row += $" FROM {fromTable}";
            }

            var rows = string.Join(" UNION ALL ", Enumerable.Repeat(row, adHoc.Values.Count / adHoc.Columns.Count));
            ctx.Bindings.AddRange(adHoc.Values);
            ctx.Write(rows);
        }

        public virtual void VisitJoins(CompileContext ctx, Query query)
        {
            var joins = query.Joins.ForEngine(EngineCode).ToList();
            if (!joins.Any()) return;

            ctx.EnsureSpace();
            for (var i = 0; i < joins.Count; i++)
            {
                if (i > 0)
                {
                    ctx.Write("\n");
                }
                else if (ctx.Buffer.Length > 0 && ctx.Buffer[ctx.Buffer.Length - 1] == ' ')
                {
                    ctx.Write("\n");
                }
                VisitJoin(ctx, joins[i].Join);
            }
        }

        public virtual void VisitJoin(CompileContext ctx, Join join, bool isNested = false)
        {
            var from = join.FromClause;
            var conditions = join.Conditions.GetConditions(EngineCode);

            ctx.Write(join.Type).Write(" ");
            VisitTableExpression(ctx, from);

            if (conditions.Any())
            {
                ctx.Write(" ON ");
                VisitConditions(ctx, conditions);
            }
        }

        public virtual void VisitWheres(CompileContext ctx, Query query) =>
            VisitConditionsClause(ctx, "WHERE", query.Wheres);

        public virtual void VisitHaving(CompileContext ctx, Query query) =>
            VisitConditionsClause(ctx, "HAVING", query.Havings);

        protected virtual void VisitConditionsClause(CompileContext ctx, string keyword, ConditionGroup group)
        {
            if (!group.HasConditions(EngineCode))
            {
                return;
            }

            var before = ctx.Buffer.Length;
            ctx.EnsureSpace();
            ctx.Write(keyword).Write(" ");
            var beforeConds = ctx.Buffer.Length;

            var conditions = group.Clauses
                .ForEngine(EngineCode)
                .Cast<AbstractCondition>()
                .ToList();

            VisitConditions(ctx, conditions);

            if (ctx.Buffer.Length == beforeConds)
            {
                ctx.Buffer.Length = before;
            }
        }

        public virtual void VisitGroups(CompileContext ctx, Query query)
        {
            var columns = query.GroupByClauses.ForEngine(EngineCode).ToList();
            if (!columns.Any())
            {
                return;
            }

            ctx.EnsureSpace().Write("GROUP BY ");
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0) ctx.Write(", ");
                VisitColumn(ctx, columns[i]);
            }
        }

        public virtual void VisitOrders(CompileContext ctx, Query query)
        {
            var columns = query.OrderByClauses.ForEngine(EngineCode).ToList();
            if (!columns.Any())
            {
                return;
            }

            ctx.EnsureSpace().Write("ORDER BY ");
            for (var i = 0; i < columns.Count; i++)
            {
                if (i > 0) ctx.Write(", ");
                var x = columns[i];
                if (x is RawOrderBy raw)
                {
                    ctx.Bindings.AddRange(raw.Bindings);
                    ctx.Write(WrapIdentifiers(raw.Expression));
                }
                else if (x is OrderBy orderBy)
                {
                    var direction = orderBy.Ascending ? "" : " DESC";
                    ctx.Write(Wrap(orderBy.Column)).Write(direction);
                }
            }
        }

        public virtual void VisitLimit(CompileContext ctx, Query query)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }

            ctx.EnsureSpace();

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                ctx.Write($"LIMIT {parameterPlaceholder}");
                return;
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                ctx.Write($"OFFSET {parameterPlaceholder}");
                return;
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);
            ctx.Write($"LIMIT {parameterPlaceholder} OFFSET {parameterPlaceholder}");
        }

        public virtual void VisitUnions(CompileContext ctx, Query query)
        {
            var clauses = query.CombineClauses.ForEngine(EngineCode).ToList();
            if (!clauses.Any())
            {
                return;
            }

            foreach (var clause in clauses)
            {
                ctx.EnsureSpace();
                if (clause is Combine combineClause)
                {
                    var combineOperator = combineClause.Operation.ToUpperInvariant() + " " + (combineClause.All ? "ALL " : "");
                    ctx.Write(combineOperator);
                    CompileSubQuery(ctx, combineClause.Query);
                }
                else if (clause is RawCombine rawCombine)
                {
                    ctx.Bindings.AddRange(rawCombine.Bindings);
                    ctx.Write(WrapIdentifiers(rawCombine.Expression));
                }
            }
        }

        // ==========================================
        // Backward Compatibility Wrappers
        // ==========================================

        protected virtual CompileContext OnBeforeSelect(CompileContext ctx) => ctx;

        public virtual string CompileLimit(CompileContext ctx)
        {
            var prevLen = ctx.Buffer.Length;
            VisitLimit(ctx, ctx.Query);
            if (ctx.Buffer.Length > prevLen)
            {
                var result = ctx.Buffer.ToString(prevLen, ctx.Buffer.Length - prevLen).Trim();
                ctx.Buffer.Length = prevLen;
                return result;
            }
            return null;
        }

        public virtual string CompileOrders(CompileContext ctx)
        {
            var prevLen = ctx.Buffer.Length;
            VisitOrders(ctx, ctx.Query);
            if (ctx.Buffer.Length > prevLen)
            {
                var result = ctx.Buffer.ToString(prevLen, ctx.Buffer.Length - prevLen).Trim();
                ctx.Buffer.Length = prevLen;
                return result;
            }
            return null;
        }

        // ==========================================
        // Helper / Formatting Methods
        // ==========================================

        protected string GetInsertColumnsList(List<string> columnList)
        {
            var columns = "";
            if (columnList.Any())
                columns = $" ({string.Join(", ", WrapArray(columnList))})";

            return columns;
        }

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

        public virtual string WrapValue(string value)
        {
            if (value == "*") return value;

            var opening = this.OpeningIdentifier;
            var closing = this.ClosingIdentifier;

            if (string.IsNullOrWhiteSpace(opening) && string.IsNullOrWhiteSpace(closing)) return value;

            return opening + value.Replace(closing, closing + closing) + closing;
        }

        public virtual object Resolve(CompileContext ctx, object parameter)
        {
            if (parameter is UnsafeLiteral literal)
            {
                return literal.Value;
            }

            if (parameter is Variable variable)
            {
                var value = ctx.FindVariable(variable.Name);
                return value;
            }

            return parameter;
        }

        public virtual string Parameter(CompileContext ctx, object parameter)
        {
            if (parameter is UnsafeLiteral literal)
            {
                return literal.Value;
            }

            if (parameter is Variable variable)
            {
                var value = ctx.FindVariable(variable.Name);
                ctx.Bindings.Add(value);
                return parameterPlaceholder;
            }

            ctx.Bindings.Add(parameter);
            return parameterPlaceholder;
        }

        public virtual string Parameterize<T>(CompileContext ctx, IEnumerable<T> values)
        {
            return string.Join(", ", values.Select(x => Parameter(ctx, x)));
        }

        public virtual List<string> WrapArray(List<string> values)
        {
            return values.Select(x => Wrap(x)).ToList();
        }

        public virtual string WrapIdentifiers(string input)
        {
            return input
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "{", this.OpeningIdentifier)
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "}", this.ClosingIdentifier)
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "[", this.OpeningIdentifier)
                .ReplaceIdentifierUnlessEscaped(this.EscapeCharacter, "]", this.ClosingIdentifier);
        }
    }
}
