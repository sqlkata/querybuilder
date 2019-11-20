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

        protected Compiler()
        {
            _compileConditionMethodsProvider = new ConditionsCompilerProvider(this);
        }

        public virtual string EngineCode { get; }


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

        protected SqlResult PrepareResult(SqlResult context)
        {
            context.NamedBindings = generateNamedBindings(context.Bindings.ToArray());
            context.Sql = Helper.ReplaceAll(context.RawSql, parameterPlaceholder, i => parameterPrefix + i);
            return context;
        }


        private Query TransformAggregateQuery(Query query)
        {
            AggregateClause clause = query.GetOneComponent<AggregateClause>("aggregate", EngineCode);

            if (clause.Columns.Count == 1 && !query.IsDistinct)
            {
                return query;
            }

            if (query.IsDistinct)
            {
                query.ClearComponent("aggregate", EngineCode);
                query.ClearComponent("select", EngineCode);
                query.Select(clause.Columns.ToArray());
            }
            else
            {
                foreach (string column in clause.Columns)
                {
                    query.WhereNotNull(column);
                }
            }

            AggregateClause outerClause = new AggregateClause()
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
            SqlResult context;

            if (query.Method == "insert")
            {
                context = CompileInsertQuery(query);
            }
            else if (query.Method == "update")
            {
                context = CompileUpdateQuery(query);
            }
            else if (query.Method == "delete")
            {
                context = CompileDeleteQuery(query);
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

                context = CompileSelectQuery(query);
            }

            // handle CTEs
            if (query.HasComponent("cte", EngineCode))
            {
                context = CompileCteQuery(context, query);
            }

            context.RawSql = Helper.ExpandParameters(context.RawSql, "?", context.Bindings.ToArray());

            return context;
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
            foreach (string op in operators)
            {
                this.userOperators.Add(op);
            }

            return this;
        }

        public virtual SqlResult Compile(Query query)
        {
            SqlResult context = CompileRaw(query);

            context = PrepareResult(context);

            return context;
        }

        public virtual SqlResult Compile(IEnumerable<Query> queries)
        {
            SqlResult[] compiled = queries.Select(CompileRaw).ToArray();
            List<object>[] bindings = compiled.Select(r => r.Bindings).ToArray();
            int totalBindingsCount = bindings.Select(b => b.Count).Aggregate((a, b) => a + b);

            List<object> combinedBindings = new List<object>(totalBindingsCount);
            foreach (List<object> binding in bindings)
            {
                combinedBindings.AddRange(binding);
            }

            SqlResult context = new SqlResult
            {
                RawSql = compiled.Select(r => r.RawSql).Aggregate((a, b) => a + ";\n" + b),
                Bindings = combinedBindings,
            };

            context = PrepareResult(context);

            return context;
        }

        protected virtual SqlResult CompileSelectQuery(Query query)
        {
            SqlResult context = new SqlResult
            {
                Query = query.Clone(),
            };

            List<string> results = new[] {
                    this.CompileColumns(context),
                    this.CompileFrom(context),
                    this.CompileJoins(context),
                    this.CompileWheres(context),
                    this.CompileGroups(context),
                    this.CompileHaving(context),
                    this.CompileOrders(context),
                    this.CompileLimit(context),
                    this.CompileUnion(context),
                }
               .Where(x => x != null)
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            string sql = string.Join(" ", results);

            context.RawSql = sql;

            return context;
        }

        private SqlResult IntializeSQL_Result(Query query, string errorMsg)
        {
            SqlResult context = new SqlResult
            {
                Query = query
            };

            if (!context.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to " + errorMsg);
            }

            return context;
        }
        private string MakeTable(SqlResult context)
        {
            AbstractFrom fromClause = context.Query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (fromClause is null)
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            string table = null;
            
            if (fromClause is FromClause fromClauseCast)
            {
                table = Wrap(fromClauseCast.Table);
            }
            else if (fromClause is RawFromClause rawFromClause)
            {
                table = WrapIdentifiers(rawFromClause.Expression);
                context.Bindings.AddRange(rawFromClause.Bindings);
            }

            if (table is null)
            {
                throw new InvalidOperationException("Invalid table expression");
            }
            
            return table;
        }

        private SqlResult CompileDeleteQuery(Query query)
        {
            SqlResult context = IntializeSQL_Result(query,"delete");

            string table = MakeTable(context);

            string where = CompileWheres(context);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            context.RawSql = $"DELETE FROM {table}{where}";

            return context;
        }
        private List<string> MakeUpdateParts(InsertClause toUpdate)
        {
            List<string> parts = new List<string>();

            for (int column = 0; column < toUpdate.Columns.Count; column++)
            {
                parts.Add($"{Wrap(toUpdate.Columns[column])} = ?");
            }
            return parts;
        }
        private SqlResult CompileUpdateQuery(Query query)
        {
            SqlResult context = IntializeSQL_Result(query, "update");

            string table = MakeTable(context);

            InsertClause toUpdate = context.Query.GetOneComponent<InsertClause>("update", EngineCode);

            List<string> parts = MakeUpdateParts(toUpdate);

            context.Bindings.AddRange(toUpdate.Values);

            string where = CompileWheres(context);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            string sets = string.Join(", ", parts);

            context.RawSql = $"UPDATE {table} SET {sets}{where}";

            return context;
        }

        private SqlResult IfInsertCase(SqlResult context, string table, InsertClause insertClause)
        {
            string columns = string.Join(", ", WrapArray(insertClause.Columns));
            string values = string.Join(", ", Parameterize(context, insertClause.Values));

            context.RawSql = $"INSERT INTO {table} ({columns}) VALUES ({values})";

            if (insertClause.ReturnId && !string.IsNullOrEmpty(LastId))
            {
                context.RawSql += ";" + LastId;
            }

            return context;
        }

        protected virtual SqlResult CompileInsertQuery(Query query)
        {
            SqlResult context = IntializeSQL_Result(query, " insert");

            string table = MakeTable(context);

            List<AbstractInsertClause> inserts = context.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts[0] is InsertClause insertClause)
            {
                context = IfInsertCase(context, table, insertClause);
            }
            else
            {
                ElseInsertCase(context, table, inserts);
            }

            if (inserts.Count > 1)
            {
                foreach (InsertClause clause in inserts.GetRange(1, inserts.Count - 1))
                {

                    context.RawSql += ", (" + string.Join(", ", Parameterize(context, clause.Values)) + ")";

                }
            }


            return context;
        }

        private void ElseInsertCase(SqlResult context, string table, List<AbstractInsertClause> inserts)
        {
            InsertQueryClause clause = inserts[0] as InsertQueryClause;

            string columns = "";

            if (clause.Columns.Any())
            {
                columns = $" ({string.Join(", ", WrapArray(clause.Columns))}) ";
            }

            SqlResult subContext = CompileSelectQuery(clause.Query);
            context.Bindings.AddRange(subContext.Bindings);

            context.RawSql = $"INSERT INTO {table}{columns}{subContext.RawSql}";
        }

        protected virtual SqlResult CompileCteQuery(SqlResult context, Query query)
        {
            CteFinder cteFinder = new CteFinder(query, EngineCode);
            List<AbstractFrom> cteSearchResult = cteFinder.Find();

            StringBuilder rawSql = new StringBuilder("WITH ");
            List<object> cteBindings = new List<object>();

            foreach (AbstractFrom cte in cteSearchResult)
            {
                SqlResult cteContext = CompileCte(cte);

                cteBindings.AddRange(cteContext.Bindings);
                rawSql.Append(cteContext.RawSql.Trim());
                rawSql.Append(",\n");
            }

            rawSql.Length -= 2; // remove last comma
            rawSql.Append('\n');
            rawSql.Append(context.RawSql);

            context.Bindings.InsertRange(0, cteBindings);
            context.RawSql = rawSql.ToString();

            return context;
        }

        /// <summary>
        /// Compile a single column clause
        /// </summary>
        /// <param name="context"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string CompileColumn(SqlResult context, AbstractColumn column)
        {
            if (column is RawColumn raw)
            {
                context.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (column is QueryColumn queryColumn)
            {
                string alias = "";

                if (!string.IsNullOrWhiteSpace(queryColumn.Query.QueryAlias))
                {
                    alias = $" {ColumnAsKeyword}{WrapValue(queryColumn.Query.QueryAlias)}";
                }

                SqlResult subContext = CompileSelectQuery(queryColumn.Query);

                context.Bindings.AddRange(subContext.Bindings);

                return "(" + subContext.RawSql + $"){alias}";
            }

            return Wrap((column as Column).Name);

        }


        public virtual SqlResult CompileCte(AbstractFrom context)
        {
            SqlResult sqlContext = new SqlResult();

            if (null == context)
            {
                return sqlContext;
            }

            if (context is RawFromClause raw)
            {
                sqlContext.Bindings.AddRange(raw.Bindings);
                sqlContext.RawSql = $"{WrapValue(raw.Alias)} AS ({WrapIdentifiers(raw.Expression)})";
            }
            else if (context is QueryFromClause queryFromClause)
            {
                SqlResult subSqlContext = CompileSelectQuery(queryFromClause.Query);
                sqlContext.Bindings.AddRange(subSqlContext.Bindings);

                sqlContext.RawSql = $"{WrapValue(queryFromClause.Alias)} AS ({subSqlContext.RawSql})";
            }

            return sqlContext;
        }

        protected virtual SqlResult OnBeforeSelect(SqlResult context)
        {
            return context;
        }

        protected virtual string CompileColumns(SqlResult context)
        {
            if (context.Query.HasComponent("aggregate", EngineCode))
            {
                AggregateClause aggregate = context.Query.GetOneComponent<AggregateClause>("aggregate", EngineCode);

                List<string> aggregateColumns = aggregate.Columns
                    .Select(x => CompileColumn(context, new Column { Name = x }))
                    .ToList();

                string sql = string.Empty;

                if (aggregateColumns.Count == 1)
                {
                    sql = string.Join(", ", aggregateColumns);

                    if (context.Query.IsDistinct)
                    {
                        sql = "DISTINCT " + sql;
                    }

                    return "SELECT " + aggregate.Type.ToUpperInvariant() + "(" + sql + $") {ColumnAsKeyword}" + WrapValue(aggregate.Type);
                }

                return "SELECT 1";
            }

            List<string> columns = context.Query
                .GetComponents<AbstractColumn>("select", EngineCode)
                .Select(x => CompileColumn(context, x))
                .ToList();

            string distinct = context.Query.IsDistinct ? "DISTINCT " : "";

            string select = columns.Any() ? string.Join(", ", columns) : "*";

            return $"SELECT {distinct}{select}";

        }

        public virtual string CompileUnion(SqlResult context)
        {

            // Handle UNION, EXCEPT and INTERSECT
            if (!context.Query.GetComponents("combine", EngineCode).Any())
            {
                return null;
            }

            List<string> combinedQueries = new List<string>();

            List<AbstractCombine> clauses = context.Query.GetComponents<AbstractCombine>("combine", EngineCode);

            foreach (AbstractCombine clause in clauses)
            {
                if (clause is Combine combineClause)
                {
                    string combineOperator = combineClause.Operation.ToUpperInvariant() + " " + (combineClause.All ? "ALL " : "");

                    SqlResult subContext = CompileSelectQuery(combineClause.Query);

                    context.Bindings.AddRange(subContext.Bindings);

                    combinedQueries.Add($"{combineOperator}{subContext.RawSql}");
                }
                else
                {
                    RawCombine combineRawClause = clause as RawCombine;

                    context.Bindings.AddRange(combineRawClause.Bindings);

                    combinedQueries.Add(WrapIdentifiers(combineRawClause.Expression));

                }
            }

            return string.Join(" ", combinedQueries);

        }

        public virtual string CompileTableExpression(SqlResult context, AbstractFrom from)
        {
            if (from is RawFromClause raw)
            {
                context.Bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (from is QueryFromClause queryFromClause)
            {
                Query fromQuery = queryFromClause.Query;

                string alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : $" {TableAsKeyword}" + WrapValue(fromQuery.QueryAlias);

                SqlResult subContext = CompileSelectQuery(fromQuery);

                context.Bindings.AddRange(subContext.Bindings);

                return "(" + subContext.RawSql + ")" + alias;
            }

            if (from is FromClause fromClause)
            {
                return Wrap(fromClause.Table);
            }

            throw InvalidClauseException("TableExpression", from);
        }

        public virtual string CompileFrom(SqlResult context)
        {
            if (!context.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table is set");
            }

            AbstractFrom from = context.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            return "FROM " + CompileTableExpression(context, from);
        }

        public virtual string CompileJoins(SqlResult context)
        {
            if (!context.Query.HasComponent("join", EngineCode))
            {
                return null;
            }

            IEnumerable<string> joins = context.Query
                .GetComponents<BaseJoin>("join", EngineCode)
                .Select(x => CompileJoin(context, x.Join));

            return "\n" + string.Join("\n", joins);
        }

        public virtual string CompileJoin(SqlResult context, Join join, bool isNested = false)
        {

            AbstractFrom from = join.GetOneComponent<AbstractFrom>("from", EngineCode);
            List<AbstractCondition> conditions = join.GetComponents<AbstractCondition>("where", EngineCode);

            string joinTable = CompileTableExpression(context, from);
            string constraints = CompileConditions(context, conditions);

            string onClause = conditions.Any() ? $" ON {constraints}" : "";

            return $"{join.Type} {joinTable}{onClause}";
        }

        public virtual string CompileWheres(SqlResult context)
        {
            if (!context.Query.HasComponent("from", EngineCode) || !context.Query.HasComponent("where", EngineCode))
            {
                return null;
            }

            List<AbstractCondition> conditions = context.Query.GetComponents<AbstractCondition>("where", EngineCode);
            string sql = CompileConditions(context, conditions).Trim();

            return string.IsNullOrEmpty(sql) ? null : $"WHERE {sql}";
        }

        public virtual string CompileGroups(SqlResult context)
        {
            if (!context.Query.HasComponent("group", EngineCode))
            {
                return null;
            }

            IEnumerable<string> columns = context.Query
                .GetComponents<AbstractColumn>("group", EngineCode)
                .Select(x => CompileColumn(context, x));

            return "GROUP BY " + string.Join(", ", columns);
        }

        public virtual string CompileOrders(SqlResult context)
        {
            if (!context.Query.HasComponent("order", EngineCode))
            {
                return null;
            }

            IEnumerable<string> columns = context.Query
                .GetComponents<AbstractOrderBy>("order", EngineCode)
                .Select(orderBy =>
            {

                if (orderBy is RawOrderBy raw)
                {
                    context.Bindings.AddRange(raw.Bindings);
                    return WrapIdentifiers(raw.Expression);
                }

                string direction = (orderBy as OrderBy).Ascending ? "" : " DESC";

                return Wrap((orderBy as OrderBy).Column) + direction;
            });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public virtual string CompileHaving(SqlResult context)
        {
            if (!context.Query.HasComponent("having", EngineCode))
            {
                return null;
            }

            List<string> sql = new List<string>();
            string boolOperator;

            List<AbstractCondition> having = context.Query.GetComponents("having", EngineCode)
                .Cast<AbstractCondition>()
                .ToList();

            for (int i = 0; i < having.Count; i++)
            {
                string compiled = CompileCondition(context, having[i]);

                if (!string.IsNullOrEmpty(compiled))
                {
                    boolOperator = i > 0 ? having[i].IsOr ? "OR " : "AND " : "";

                    sql.Add(boolOperator + compiled);
                }
            }

            return $"HAVING {string.Join(" ", sql)}";
        }

        public virtual string CompileLimit(SqlResult context)
        {
            int limit = context.Query.GetLimit(EngineCode);
            int offset = context.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                context.Bindings.Add(limit);
                return "LIMIT ?";
            }

            if (limit == 0)
            {
                context.Bindings.Add(offset);
                return "OFFSET ?";
            }

            context.Bindings.Add(limit);
            context.Bindings.Add(offset);

            return "LIMIT ? OFFSET ?";
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

            bool valid = operators.Contains(op) || userOperators.Contains(op);

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
                int index = value.ToLowerInvariant().IndexOf(" as ");
                string before = value.Substring(0, index);
                string after = value.Substring(index + 4);

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

            string opening = this.OpeningIdentifier;
            string closing = this.ClosingIdentifier;

            return opening + value.Replace(closing, closing + closing) + closing;
        }

        /// <summary>
        /// Resolve a parameter
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual object Resolve(SqlResult context, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal)
            {
                return literal.Value;
            }

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
            {
                object value = context.Query.FindVariable(variable.Name);
                return value;
            }

            return parameter;

        }

        /// <summary>
        /// Resolve a parameter and add it to the binding list
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual string Parameter(SqlResult context, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal)
            {
                return literal.Value;
            }

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
            {
                object value = context.Query.FindVariable(variable.Name);
                context.Bindings.Add(value);
                return "?";
            }

            context.Bindings.Add(parameter);
            return "?";
        }

        /// <summary>
        /// Create query parameter place-holders for an array.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual string Parameterize<T>(SqlResult context, IEnumerable<T> values)
        {
            return string.Join(", ", values.Select(x => Parameter(context, x)));
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
