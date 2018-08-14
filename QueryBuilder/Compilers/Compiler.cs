using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{

    public partial class Compiler
    {
        private readonly CompileConditionMethods compileConditionMethodsProvider;

        public string EngineCode;
        protected string OpeningIdentifier = "\"";
        protected string ClosingIdentifier = "\"";
        protected string ColumnAsKeyword = "AS ";
        protected string TableAsKeyword = "AS ";

        public Compiler()
        {
            compileConditionMethodsProvider = new CompileConditionMethods(GetType());
        }

        public virtual SqlResult Compile(Query query)
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
                }

                ctx = CompileSelectQuery(query);
            }

            // handle CTEs
            if (query.HasComponent("cte", EngineCode))
            {
                var cteCtx = CompileCte(query.GetComponents<AbstractFrom>("cte", EngineCode));
                ctx.Bindings.InsertRange(0, cteCtx.Bindings);
                ctx.RawSql = cteCtx.RawSql.Trim() + "\n" + ctx.RawSql;
            }


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

        private SqlResult CompileDeleteQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to delete");
            }

            var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            if (!(from is FromClause))
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            var where = CompileWheres(ctx);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            ctx.RawSql = "DELETE FROM " + CompileTableExpression(ctx, from) + where;

            return ctx;
        }

        private SqlResult CompileUpdateQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to update");
            }

            var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            if (!(from is FromClause))
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            var toUpdate = ctx.Query.GetOneComponent<InsertClause>("update", EngineCode);

            var parts = new List<string>();

            for (var i = 0; i < toUpdate.Columns.Count; i++)
            {
                parts.Add($"{Wrap(toUpdate.Columns[i])} = ?");
            }

            ctx.Bindings.AddRange(toUpdate.Values);

            var where = CompileWheres(ctx);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            ctx.RawSql = "UPDATE " + CompileTableExpression(ctx, from)
                + " SET " + string.Join(", ", parts)
                + where;

            return ctx;
        }

        protected virtual SqlResult CompileInsertQuery(Query query)
        {
            var ctx = new SqlResult
            {
                Query = query
            };

            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to insert");
            }

            var fromClause = ctx.Query.GetOneComponent<FromClause>("from", EngineCode);

            if (fromClause is null)
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            var table = Wrap(fromClause.Table);

            var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts[0] is InsertClause insertClause)
            {
                ctx.RawSql = $"INSERT INTO {table}"
                    + " (" + string.Join(", ", WrapArray(insertClause.Columns)) + ") "
                    + "VALUES (" + string.Join(", ", Parameterize(ctx, insertClause.Values)) + ")";
            }
            else
            {
                var clause = inserts[0] as InsertQueryClause;

                var columns = "";

                if (clause.Columns.Any())
                {
                    columns = $" ({string.Join(", ", WrapArray(clause.Columns))}) ";
                }

                var subCtx = CompileSelectQuery(clause.Query);
                ctx.Bindings.AddRange(subCtx.Bindings);

                ctx.RawSql = $"INSERT INTO {table}{columns}{subCtx.RawSql}";
            }

            if (inserts.Count > 1)
            {
                foreach (var insert in inserts.GetRange(1, inserts.Count - 1))
                {
                    var clause = insert as InsertClause;

                    ctx.RawSql += ", (" + string.Join(", ", Parameterize(ctx, clause.Values)) + ")";

                }
            }


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

            return Wrap((column as Column).Name);

        }

        protected virtual SqlResult OnBeforeCompile(SqlResult ctx)
        {
            return ctx;
        }

        public virtual string OnAfterCompile(string sql)
        {
            return sql;
        }

        public virtual SqlResult CompileCte(List<AbstractFrom> cteClauses)
        {

            var ctx = new SqlResult { };

            if (!cteClauses.Any())
            {
                return ctx;
            }

            var sql = new List<string>();

            foreach (var cte in cteClauses)
            {
                if (cte is RawFromClause raw)
                {
                    ctx.Bindings.AddRange(raw.Bindings);
                    sql.Add($"{WrapValue(raw.Alias)} AS ({WrapIdentifiers(raw.Expression)})");
                }
                else if (cte is QueryFromClause queryFromClause)
                {
                    var subCtx = CompileSelectQuery(queryFromClause.Query);
                    ctx.Bindings.AddRange(subCtx.Bindings);

                    sql.Add($"{WrapValue(queryFromClause.Alias)} AS ({subCtx.RawSql})");
                }
            }

            ctx.RawSql = "WITH " + string.Join(",\n", sql) + " ";
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

                var sql = string.Join(", ", aggregateColumns);

                if (ctx.Query.IsDistinct)
                {
                    sql = "DISTINCT " + sql;
                }

                return "SELECT " + aggregate.Type.ToUpper() + "(" + sql + $") {ColumnAsKeyword}" + WrapValue(aggregate.Type);
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
                    var combineOperator = combineClause.Operation.ToUpper() + " " + (combineClause.All ? "ALL " : "");

                    var subCtx = CompileSelectQuery(combineClause.Query);

                    ctx.Bindings.AddRange(subCtx.Bindings);

                    combinedQueries.Add($"{combineOperator}({subCtx.RawSql})");
                }
                else
                {
                    var combineRawClause = clause as RawCombine;

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
            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table is set");
            }

            var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            return "FROM " + CompileTableExpression(ctx, from);
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
            if (!ctx.Query.HasComponent("from", EngineCode) || !ctx.Query.HasComponent("where", EngineCode))
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

                var direction = (x as OrderBy).Ascending ? "" : " DESC";

                return Wrap((x as OrderBy).Column) + direction;
            });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public string CompileHaving(SqlResult ctx)
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

                    sql.Add(boolOperator + "HAVING " + compiled);
                }
            }

            return string.Join(", ", sql);
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
                return "LIMIT ?";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return "OFFSET ?";
            }

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return "LIMIT ? OFFSET ?";
        }

        // public virtual string CompileOffset(SqlResult ctx)
        // {
        //     if (ctx.Query.GetOneComponent("limit", EngineCode) is LimitOffset limitOffset && limitOffset.HasOffset())
        //     {
        //         ctx.Bindings.Add(limitOffset.Offset);
        //         return "OFFSET ?";
        //     }

        //     return null;
        // }

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

        /// <summary>
        /// Wrap a single string in a column identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string Wrap(string value)
        {
            if (value.ToLower().Contains(" as "))
            {
                var index = value.ToLower().IndexOf(" as ");
                var before = value.Substring(0, index);
                var after = value.Substring(index + 4);

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

            return opening + value.Replace(closing, closing + closing) + closing;
        }

        public virtual string Parameter<T>(SqlResult ctx, T value)
        {
            ctx.Bindings.Add(value);
            return "?";
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
                .Replace("{", this.OpeningIdentifier)
                .Replace("}", this.ClosingIdentifier)

                .Replace("[", this.OpeningIdentifier)
                .Replace("]", this.ClosingIdentifier);
        }

    }



}
