using System.Collections.Generic;
using System.Linq;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        /// <summary>
        /// Wrap a single string in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string WrapValue(string value)
        {
            if (value == "*") return value;

            return '[' + value.Replace("]", "]]") + ']';
        }

        protected override Query OnBeforeSelect(Query query)
        {
            var limitOffset = query.GetOne<LimitOffset>("limit");

            if (limitOffset == null || !limitOffset.HasOffset())
            {
                return query;
            }


            // Surround the original query with a parent query, then restrict the result to the offset provided, see more at https://docs.microsoft.com/en-us/sql/t-sql/functions/row-number-transact-sql


            var rowNumberColName = Wrap("row_num");

            var orderStatement = CompileOrders(query) ?? "ORDER BY (SELECT 0)";



            // get a clone without the limit and order
            query.Clear("order");
            query.Clear("limit");
            var cloned = query.Clone();

            // Now clear other stuff
            query.Clear("select");
            query.Clear("from");
            query.Clear("join");
            query.Clear("where");
            query.Clear("group");
            query.Clear("having");
            query.Clear("union");

            // Transform the query to make it a parent query
            query.Select("*");

            if (!cloned.Has("columns"))
            {
                cloned.SelectRaw("*");
            }

            cloned.SelectRaw($"ROW_NUMBER() OVER ({orderStatement}) AS {rowNumberColName}");

            query.From(cloned);

            if (limitOffset.HasLimit())
            {

                var bindings = new object[] {
                        limitOffset.Offset + 1,
                        limitOffset.Limit + limitOffset.Offset,
                    };

                query.WhereRaw($"{rowNumberColName} BETWEEN ? AND ?", bindings);
            }
            else
            {
                var bindings = new object[] {
                         limitOffset.Offset + 1
                    };

                query.WhereRaw($"{rowNumberColName} >= ?", bindings);
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
            var limitOffset = query.GetOne("limit") as LimitOffset;

            if (limitOffset != null && limitOffset.HasLimit() && !limitOffset.HasOffset())
            {
                // Add a fake raw select to simulate the top bindings 
                query.Clauses.Insert(0, new RawColumn
                {
                    Component = "select",
                    Expression = "",
                    Bindings = new object[] { limitOffset.Limit }
                });

                query.Clear("limit");

                return "SELECT TOP ?" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(Query query)
        {
            return "";
        }

        public override string CompileOffset(Query query)
        {

            return "";
        }

        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }
    }
}