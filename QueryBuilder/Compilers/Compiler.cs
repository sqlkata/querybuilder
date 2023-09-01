using System.Collections.Immutable;
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
        protected string? EngineCode { get; init; }
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

        public void CompileRaw(Query query, Writer writer)
        {
            // handle CTEs
            if (query.HasComponent("cte", EngineCode))
            {
                CompileCteQuery(query, writer);
            }
            if (query.Method == "insert")
            {
                CompileInsertQuery(query, writer);
            }
            else if (query.Method == "update")
            {
                CompileUpdateQuery(query, writer);
            }
            else if (query.Method == "delete")
            {
                CompileDeleteQuery(query, writer);
            }
            else
            {
                if (query.Method == "aggregate")
                {
                    query.RemoveComponent("limit")
                        .RemoveComponent("order")
                        .RemoveComponent("group");

                    query = TransformAggregateQuery(query);
                }

                CompileSelectQuery(query, writer);
            }

            Query TransformAggregateQuery(Query input)
            {
                var clause = input.GetOneComponent<AggregateClause>("aggregate", EngineCode)!;

                if (clause.Columns.Length == 1 && !input.IsDistinct) return input;

                if (input.IsDistinct)
                {
                    input.RemoveComponent("aggregate", EngineCode);
                    input.RemoveComponent("select", EngineCode);
                    input.Select(clause.Columns.ToArray());
                }
                else
                {
                    foreach (var column in clause.Columns) input.WhereNotNull(column);
                }

                var outerClause = new AggregateClause
                {
                    Engine = EngineCode,
                    Component = "aggregate",
                    Columns = ImmutableArray.Create<string>().Add("*"),
                    Type = clause.Type
                };

                return new Query()
                    .AddComponent(outerClause)
                    .From(input, $"{clause.Type}Query");
            }

        }


        protected virtual void CompileSelectQuery(Query query, Writer writer)
        {
            writer.WhitespaceSeparated(
                () => CompileColumns(query, writer),
                () => CompileFrom(query, writer),
                () => CompileJoins(query, writer),
                () => CompileWheres(query, writer),
                () => CompileGroups(query, writer),
                () => CompileHaving(query, writer),
                () => CompileOrders(query, writer),
                () => CompileLimit(query, writer),
                () => CompileUnion(query, writer));
        }

        protected virtual void CompileAdHocQuery(AdHocTableFromClause adHoc, Writer writer)
        {
            Debug.Assert(adHoc.Alias != null, "adHoc.Alias != null");
            writer.AppendValue(adHoc.Alias);
            writer.Append(" AS (");
            writer.List(" UNION ALL ",
                adHoc.Values.Length / adHoc.Columns.Length, _ =>
                {
                    writer.Append("SELECT ");
                    writer.List(", ", adHoc.Columns, column =>
                    {
                        writer.Append("? AS ");
                        writer.AppendName(column);
                    });
                    if (SingleRowDummyTableName != null)
                    {
                        writer.Append(" FROM ");
                        writer.Append(SingleRowDummyTableName);
                    }
                });
            writer.BindMany(adHoc.Values);
            writer.Append(")");
        }

        private void CompileDeleteQuery(Query query, Writer writer)
        {
            if (!query.HasComponent("join", EngineCode))
            {
                writer.Append("DELETE FROM ");
                WriteTable(query, writer, "delete");
                CompileWheres(query, writer);
            }
            else
            {
                var fromClause = query.GetOneComponent<AbstractFrom>("from", EngineCode);
                if (fromClause is not FromClause c) return;

                writer.Append("DELETE ");
                writer.AppendName(c.Alias);
                writer.Append(" FROM ");
                WriteTable(fromClause, writer, "delete");
                CompileJoins(query, writer);
                CompileWheres(query, writer);
            }
        }

        private void CompileUpdateQuery(Query query, Writer writer)
        {
            writer.Append("UPDATE ");

            WriteTable(query, writer, "update");

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
                writer.AppendParameter(query,
                    Math.Abs(incrementClause.Value));
                CompileWheres(query, writer);
            }

            void CompileUpdate(InsertClause insertClause)
            {
                writer.Append(" SET ");
                writer.List(", ", insertClause.Columns, (column, i) =>
                {
                    writer.AppendName(column);
                    writer.Append(" = ");
                    writer.AppendParameter(query, insertClause.Values[i]);
                });
                CompileWheres(query, writer);
            }
        }

        protected virtual void CompileInsertQuery(Query query, Writer writer)
        {
            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            var isMultiValueInsert = inserts.OfType<InsertClause>().Skip(1).Any();

            writer.Append(isMultiValueInsert
                ? MultiInsertStartClause
                : SingleInsertStartClause);
            writer.Append(" ");
            var table = WriteTable(query, writer, "insert");

            if (inserts[0] is InsertQueryClause insertQueryClause)
            {

                CompileInsertQueryClause(insertQueryClause, writer);
                return;
            }

            CompileValueInsertClauses();
            return;


            void CompileInsertQueryClause(InsertQueryClause clause, Writer w)
            {
                w.WriteInsertColumnsList(clause.Columns);
                w.Append(" ");

                CompileSelectQuery(clause.Query, w);
            }

            void CompileValueInsertClauses()
            {
                var insertClauses = inserts.Cast<InsertClause>().ToArray();
                var firstInsert = insertClauses.First();
                writer.WriteInsertColumnsList(firstInsert.Columns);
                writer.Append(" VALUES (");
                writer.CommaSeparatedParameters(query, firstInsert.Values);
                writer.Append(")");

                if (isMultiValueInsert)
                {
                    CompileRemainingInsertClauses(query, table, writer, insertClauses);

                    return;
                }
                if (firstInsert.ReturnId && !string.IsNullOrEmpty(LastId))
                {
                    writer.Append(";");
                    writer.Append(LastId);
                }
            }
        }

        protected virtual void CompileRemainingInsertClauses(Query query, string table,
            Writer writer,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                writer.Append(", (");
                writer.CommaSeparatedParameters(query, insert.Values);
                writer.Append(")");
            }
        }

        private void CompileCteQuery(Query query, Writer writer)
        {
            writer.Append("WITH ");

            writer.List(",\n", CteFinder.Find(query, EngineCode), CompileCte);
          
            writer.Append('\n');
            return;


            void CompileCte(AbstractFrom? cte)
            {
                switch (cte)
                {
                    case RawFromClause raw:
                        Debug.Assert(raw.Alias != null, "raw.Alias != null");
                        writer.AppendValue(raw.Alias);
                        writer.Append(" AS (");
                        writer.AppendRaw(raw.Expression, raw.Bindings);
                        writer.Append(")");
                        break;
                    case QueryFromClause queryFromClause:
                        Debug.Assert(queryFromClause.Alias != null, "queryFromClause.Alias != null");
                        writer.AppendValue(queryFromClause.Alias);
                        writer.Append(" AS (");
                        CompileSelectQuery(queryFromClause.Query, writer);
                        writer.Append(")");
                        break;
                    case AdHocTableFromClause adHoc:
                        CompileAdHocQuery(adHoc, writer);
                        break;
                }
            }
        }


        private void CompileColumnList(Query query, IEnumerable<AbstractColumn> columns, Writer writer)
        {
            writer.List(", ", columns, CompileColumn);
            return;

            void CompileColumn(AbstractColumn column)
            {
                switch (column)
                {
                    case RawColumn raw:
                        writer.AppendRaw(raw.Expression, raw.Bindings);
                        return;
                    case QueryColumn queryColumn:
                        writer.Append("(");
                        CompileSelectQuery(queryColumn.Query, writer);
                        writer.Append(") ");
                        writer.AppendAsAlias(queryColumn.Query.QueryAlias);
                        return;
                    case AggregatedColumn aggregatedColumn:
                        CompileAggregatedColumn(aggregatedColumn);
                        return;
                    case Column col:
                        writer.AppendName(col.Name);
                        break;
                }

            }

            void CompileAggregatedColumn(AggregatedColumn c)
            {
                writer.AppendKeyword(c.Aggregate);

                var (col, alias) = XService.SplitAlias(
                    XService.WrapName(c.Column.Name));

                var filterConditions = GetFilterConditions(c);

                if (!filterConditions.Any())
                {
                    writer.Append("(");
                    writer.Append(col);
                    writer.Append(")");
                    writer.Append(alias);
                    return;
                }

                if (SupportsFilterClause)
                {
                    writer.Append("(");
                    writer.Append(col);
                    writer.Append(") FILTER (WHERE ");
                    CompileConditions(query, filterConditions, writer);
                    writer.Append(")");
                    writer.Append(alias);
                    return;
                }

                writer.Append("(CASE WHEN ");
                CompileConditions(query, filterConditions, writer);
                writer.Append(" THEN ");
                writer.Append(col);
                writer.Append(" END)");
                writer.Append(alias);
            }
            static List<AbstractCondition> GetFilterConditions(AggregatedColumn aggregatedColumn)
            {
                if (aggregatedColumn.Filter == null)
                    return new List<AbstractCondition>();

                return aggregatedColumn.Filter
                    .GetComponents<AbstractCondition>("where");
            }
        }

        protected virtual void CompileColumns(Query query, Writer writer)
        {
            writer.Append("SELECT ");
            CompileColumnsAfterSelect(query, writer);
        }

        protected void CompileColumnsAfterSelect(Query query, Writer writer)
        {
            var aggregate = query.GetOneComponent<AggregateClause>("aggregate", EngineCode);
            if (aggregate != null)
            {
                CompileAggregateColumns();
            }
            else
            {
                if (query.IsDistinct)
                    writer.Append("DISTINCT ");
                CompileFlatColumns(query, writer);
            }

            return;

            void CompileAggregateColumns()
            {
                if (aggregate.Columns.Length == 1)
                {
                    writer.AppendKeyword(aggregate.Type);
                    writer.Append("(");
                    if (query.IsDistinct)
                        writer.Append("DISTINCT ");
                    writer.WriteInsertColumnsList(aggregate.Columns, false);
                    writer.Append(") ");
                    writer.AppendAsAlias(aggregate.Type);
                }
                else
                {
                    writer.Append("1");
                }
            }
        }

        protected void CompileFlatColumns(Query query, Writer writer)
        {
            var columns = query
                .GetComponents<AbstractColumn>("select", EngineCode);
            if (columns.Count == 0)
            {
                writer.Append("*");
            }
            else
            {
                CompileColumnList(query, columns, writer);
            }
        }

        private void CompileUnion(Query query, Writer writer)
        {
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

                        CompileSelectQuery(combine.Query, writer);
                    }
                    else if (clause is RawCombine c)
                    {
                        writer.AppendRaw(c.Expression, c.Bindings);
                    }
                });
        }

        private void CompileTableExpression(AbstractFrom from, Writer writer)
        {
            if (from is RawFromClause raw)
            {
                writer.AppendRaw(raw.Expression, raw.Bindings);
                return;
            }

            if (from is QueryFromClause queryFromClause)
            {
                var q = queryFromClause.Query;
                writer.Append("(");
                CompileSelectQuery(q, writer);

                writer.Append(")");
                if (!string.IsNullOrEmpty(q.QueryAlias))
                {
                    writer.Append(" ");
                    writer.Append(TableAsKeyword);
                    writer.AppendValue(q.QueryAlias);
                }

                return;
            }

            if (from is FromClause fromClause)
            {
                writer.AppendName(fromClause.Table);
                return;
            }

            throw InvalidClauseException("TableExpression", from);
        }

        protected string WriteTable(Query query, Writer writer, string operationName)
        {
            return WriteTable(query.GetOneComponent<AbstractFrom>("from", EngineCode),
                writer, operationName);
        }

        private static string WriteTable(AbstractFrom? abstractFrom, Writer writer, string operationName)
        {
            switch (abstractFrom)
            {
                case null:
                    throw new InvalidOperationException($"No table set to {operationName}");

                case FromClause fromClauseCast:
                    writer.AppendName(fromClauseCast.Table);
                    return fromClauseCast.Table;
                case RawFromClause raw:
                    {
                        if (raw.Bindings.Length > 0)
                        {
                            //TODO: test!
                        }
                        writer.AppendRaw(raw.Expression, raw.Bindings);
                        return writer;
                    }
                default:
                    throw new InvalidOperationException("Invalid table expression");
            }
        }

        private void CompileFrom(Query query, Writer writer)
        {
            var from = query.GetOneComponent<AbstractFrom>("from", EngineCode);
            if (from == null) return;

            writer.Append("FROM ");
            CompileTableExpression(from, writer);
        }

        private void CompileJoins(Query query, Writer writer)
        {
            var baseJoins = query.GetComponents<BaseJoin>("join", EngineCode);
            if (!baseJoins.Any())
            {
                return;
            }

            writer.Whitespace();
            writer.Append("\n");
            writer.List("\n", baseJoins, x => CompileJoin(query, x.Join, writer));
        }

        private void CompileJoin(Query query, Join join, Writer writer)
        {
            var from = join.BaseQuery.GetOneComponent<AbstractFrom>("from", EngineCode);
            var conditions = join.BaseQuery.GetComponents<AbstractCondition>("where", EngineCode);

            Debug.Assert(from != null, nameof(from) + " != null");

            writer.Append(join.Type);
            writer.Append(" ");
            CompileTableExpression(from, writer);

            if (conditions.Any())
            {
                writer.Append(" ON ");
                CompileConditions(query, conditions, writer);
            }
        }

        private void CompileWheres(Query query, Writer writer)
        {
            var conditions = query.GetComponents<AbstractCondition>("where", EngineCode);
            if (!conditions.Any()) return;

            writer.Whitespace();
            writer.Append("WHERE ");
            CompileConditions(query, conditions, writer);
        }

        private void CompileGroups(Query query, Writer writer)
        {
            var components = query.GetComponents<AbstractColumn>("group", EngineCode);
            if (!components.Any())
            {
                return;
            }
            writer.Append("GROUP BY ");
            CompileColumnList(query, components, writer);

        }

        protected string? CompileOrders(Query query, Writer writer)
        {
            var clauses = query
                .GetComponents<AbstractOrderBy>("order", EngineCode);
            if (clauses.Count == 0) return null;

            writer.Append("ORDER BY ");
            writer.List(", ", clauses, x =>
               {
                   if (x is RawOrderBy raw)
                   {
                       writer.AppendRaw(raw.Expression, raw.Bindings);
                   }
                   else if (x is OrderBy orderBy)
                   {
                       writer.AppendName(orderBy.Column);
                       if (!orderBy.Ascending)
                           writer.Append(" DESC");
                   }
               });
            return writer;
        }

        private void CompileHaving(Query query, Writer writer)
        {
            var havingClauses = query.GetComponents("having", EngineCode);
            if (havingClauses.Count == 0) return;

            writer.Append("HAVING ");
            CompileConditions(query,
                havingClauses.Cast<AbstractCondition>().ToList(),
                writer);
        }

        protected virtual string? CompileLimit(Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            if (limit != 0)
            {
                writer.Append("LIMIT ");
                writer.AppendParameter(limit);
            }

            var offset = query.GetOffset(EngineCode);
            if (offset != 0)
            {
                writer.Whitespace();
                writer.Append("OFFSET ");
                writer.AppendParameter(offset);
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
    }

    public static class CompilerQueryExtensions
    {
        public static object? Resolve(Query query, object parameter)
        {
            // if we face a literal value we have to return it directly
            if (parameter is UnsafeLiteral literal) return literal.Value;

            // if we face a variable we have to lookup the variable from the predefined variables
            if (parameter is Variable variable)
                return query.FindVariable(variable.Name);

            return parameter;
        }
    }
}
