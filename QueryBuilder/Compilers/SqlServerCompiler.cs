using System;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public SqlServerCompiler()
        {
            EngineCode = "sqlsrv";
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
        }

        /// <summary>
        ///     Called before the <see cref="Query" /> select statement is generated
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override Query OnBeforeSelect(Query query)
        {
            var limitOffset = query.GetOneComponent<LimitOffset>("limit", EngineCode);

            if (limitOffset == null || !limitOffset.HasOffset())
            {
                return query;
            }

            // Surround the original query with a parent query, then restrict the result to the offset provided,
            // see more at https://docs.microsoft.com/en-us/sql/t-sql/functions/row-number-transact-sql
            var rowNumberColName = "row_num";

            var orderStatement = CompileOrders(query) ?? "ORDER BY (SELECT 0)";

            //var orderClause = query.GetComponents("order", EngineCode);

            // Get a clone without the limit and order
            query.ClearComponent("order");
            query.ClearComponent("limit");
            var subquery = query.Clone();

            subquery.ClearComponent("cte");

            // Now clear other stuff
            query.ClearComponent("select");
            query.ClearComponent("from");
            query.ClearComponent("join");
            query.ClearComponent("where");
            query.ClearComponent("group");
            query.ClearComponent("having");
            query.ClearComponent("union");

            // Transform the query to make it a parent query
            query.Select("*");

            if (!subquery.HasComponent("select", EngineCode))
            {
                subquery.SelectRaw("*");
            }

            // Add an alias name to the subquery
            subquery.As("subquery");

            // Add the row_number select, and put back the bindings here if any
            subquery.SelectRaw(
                $"ROW_NUMBER() OVER ({orderStatement}) AS {WrapValue(rowNumberColName)}"
            );

            query.From(subquery);

            if (limitOffset.HasLimit())
            {
                query.WhereBetween(
                    rowNumberColName,
                    limitOffset.Offset + 1,
                    limitOffset.Limit + limitOffset.Offset
                );
            }
            else
            {
                query.Where(rowNumberColName, ">=", limitOffset.Offset + 1);
            }

            limitOffset.Clear();

            return query;
        }

        protected override string CompileColumns(Query query)
        {
            var compiled = base.CompileColumns(query);

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.

            if (query.GetOneComponent("limit", EngineCode) is LimitOffset limitOffset && limitOffset.HasLimit() &&
                !limitOffset.HasOffset())
            {
                // top bindings should be inserted first
                bindings.Insert(0, limitOffset.Limit);

                query.ClearComponent("limit");

                return "SELECT TOP (?)" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(Query query)
        {
            return string.Empty;
        }

        public override string CompileOffset(Query query)
        {
            return string.Empty;
        }

        /// <summary>
        ///     Returns a random id
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }

        protected override string CompileBasicDateCondition(BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            switch (condition.Part)
            {
                case "time":
                    left = $"CAST({column} as time)";
                    break;

                case "date":
                    left = $"CAST({column} as date)";
                    break;

                default:
                    left = $"DATEPART({condition.Part.ToUpper()}, {column})";
                    break;
            }

            var sql = $"{left} {condition.Operator} {Parameter(condition.Value)}";

            return condition.IsNot ? $"NOT ({sql})" : sql;
        }

        /// <summary>
        ///     Returns the table expression
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public override string CompileTableExpression(AbstractFrom from)
        {
            if (from is RawFromClause raw)
            {
                bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            var hints = string.Empty;
            if (from.Hints?.Length > 0)
            {
                hints = $" WITH ({string.Join(", ", from.Hints)})";
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;
                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : " AS " + WrapValue(fromQuery.QueryAlias);
                var compiled = CompileSelect(fromQuery);
                return "(" + compiled + ")" + alias + hints;
            }

            if (from is FromClause fromClause)
            {
                return Wrap(fromClause.Table + hints);
            }

            throw InvalidClauseException("TableExpression", from);
        }
    }

    public static class SqlServerCompilerExtensions
    {
        public static string ENGINE_CODE = "sqlsrv";

        public static Query ForSqlServer(this Query src, Func<Query, Query> fn)
        {
            return src.For(ENGINE_CODE, fn);
        }
    }
}