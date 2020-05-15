using System;
using System.Linq;

namespace SqlKata.Compilers
{
    public class AccessCompiler : Compiler
    {
        public AccessCompiler() 
        {
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";

            LastId = "SELECT @@IDENTITY as Id";
        }

        public override string EngineCode => EngineCodes.MSAccess;

        protected override SqlResult CompileSelectQuery(Query query)
        {
            if (!query.HasOffset())
            {
                return base.CompileSelectQuery(query);
            }

            if (!query.HasComponent("order"))
                throw new NotSupportedException("Offset is only supported, if there is an order by.");

            var offset = query.GetOffset();
            var limit = query.GetLimit();

            query = query.Clone();
            query.Limit(limit + offset);
            query.Offset(0);

            var outer1 = new Query();
            outer1.Limit(limit);
            outer1.From(query, "sub");
            CopyOrderBys(query, outer1, "sub", true);

            var outer2 = new Query();
            outer2.From(outer1, "subOrdered");
            CopyOrderBys(query, outer2, "subOrdered", false);

            var result = base.CompileSelectQuery(outer2);
            return result;
        }

        private static void CopyOrderBys(Query from, Query to, string tabName, bool invertAscending)
        {
            foreach (var order in from.GetComponents("order"))
            {
                var orderBy = order.Clone() as OrderBy;
                if (orderBy != null)
                {
                    if (invertAscending)
                        orderBy.Ascending = !orderBy.Ascending;

                    var colName = orderBy.Column;
                    orderBy.Column = tabName + "." + colName.Substring(colName.LastIndexOf(".") + 1);
                    to.AddComponent("order", orderBy);
                }
            }
        }

        public override string CompileFrom(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table is set");
            }

            var from = ctx.Query.GetOneComponent<AbstractFrom>("from", EngineCode);

            var joins = ctx.Query
                .GetComponents<BaseJoin>("join", EngineCode)
                .Count();

            return "FROM " + string.Join("", Enumerable.Repeat("(", joins)) + CompileTableExpression(ctx, from);
        }

        public override string CompileJoins(SqlResult ctx)
        {
            if (!ctx.Query.HasComponent("join", EngineCode))
            {
                return null;
            }

            var joins = ctx.Query
                .GetComponents<BaseJoin>("join", EngineCode)
                .Select(x => CompileJoin(ctx, x.Join) + ")");

            return "\n" + string.Join("\n", joins);
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            var compiled = base.CompileColumns(ctx);

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                // top bindings should be inserted first
                //ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT") == 0)
                {
                    return $"SELECT DISTINCT TOP {limit} {compiled.Substring(15)}";
                }

                return $"SELECT TOP {limit} {compiled.Substring(6)}";
            }

            return compiled;
        }

    }

}
