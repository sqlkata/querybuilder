using System;

namespace SqlKata
{
    public partial class Query
    {
        /// <summary>
        /// Perform a "Deep Join" with multiple tables
        /// </summary>
        /// <param name="expression">The tables separated by a "dot" i.e. "Cities.Countries.States"</param>
        /// <param name="sourceKeySuffix">The suffix for the joining key on the "source" tables, i.e. pass "_Id" if your columns are in the following format "City_Id", "Country_Id" and "State_Id"</param>
        /// <param name="targetKey">The joined table primary key, the default is "Id", i.e. Countries.Id</param>
        /// <param name="type">The join type</param>
        /// <returns></returns>
        public Query DeepJoin(
            string expression,
            string sourceKeySuffix = "Id",
            string targetKey = "Id",
            string type = "inner"
        )
        {
            return Add("join", new DeepJoin
            {
                Type = type,
                Expression = expression,
                SourceKeySuffix = sourceKeySuffix,
                TargetKey = targetKey,
            });
        }

        /// <summary>
        /// Perform a "Deep Join" based on a expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sourceKeyGenerator">Generate the joining key on the "source" tables, mainly you should return something similar to "CityId" when you join with "Cities"</param>
        /// <param name="targetKeyGenerator">Generate the key on the "target" table, if omitted the string "Id" is returned</param>
        /// <param name="type">The join type</param>
        /// <returns></returns>
        public Query DeepJoin(
            string expression,
            Func<string, string> sourceKeyGenerator,
            Func<string, string> targetKeyGenerator = null,
            string type = "inner"
        )
        {
            return Add("join", new DeepJoin
            {
                Type = type,
                Expression = expression,
                SourceKeyGenerator = sourceKeyGenerator,
                TargetKeyGenerator = targetKeyGenerator,
            });
        }

        public Query LeftDeepJoin(
            string expression,
            string sourceKeySuffix = "Id",
            string targetKey = "Id"
        )
        {
            return DeepJoin(expression, sourceKeySuffix, targetKey, "left");
        }
        public Query LeftDeepJoin(
            string expression,
            Func<string, string> sourceKeyGenerator,
            Func<string, string> targetKeyGenerator = null
        )
        {
            return DeepJoin(expression, sourceKeyGenerator, targetKeyGenerator, "left");
        }

        public Query RightDeepJoin(
            string expression,
            string sourceKeySuffix = "Id",
            string targetKey = "Id"
            )
        {
            return DeepJoin(expression, sourceKeySuffix, targetKey, "right");
        }

        public Query RightDeepJoin(
            string expression,
            Func<string, string> sourceKeyGenerator,
            Func<string, string> targetKeyGenerator = null
        )
        {
            return DeepJoin(expression, sourceKeyGenerator, targetKeyGenerator, "right");
        }

        public Query CrossDeepJoin(
             string expression,
            string sourceKeySuffix = "Id",
            string targetKey = "Id"
        )
        {
            return DeepJoin(expression, sourceKeySuffix, targetKey, "right");
        }

        public Query CrossDeepJoin(
            string expression,
            Func<string, string> sourceKeyGenerator,
            Func<string, string> targetKeyGenerator = null
        )
        {
            return DeepJoin(expression, sourceKeyGenerator, targetKeyGenerator, "cross");
        }

        private Query Join(Func<Join, Join> callback)
        {
            var join = callback.Invoke(new Join().AsInner());

            return Add("join", new BaseJoin
            {
                Join = join
            });
        }

        public Query Join(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner"
        )
        {
            return Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type));
        }

        public Query Join(string table, Func<Join, Join> callback, string type = "inner")
        {
            return Join(j => j.JoinWith(table).Where(callback).AsType(type));
        }

        public Query Join(Query query, Func<Join, Join> onCallback, string type = "inner")
        {
            return Join(j => j.JoinWith(query).Where(onCallback).AsType(type));
        }

        public Query LeftJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "left");
        }

        public Query LeftJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "left");
        }

        public Query LeftJoin(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "left");
        }

        public Query RightJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "right");
        }

        public Query RightJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "right");
        }

        public Query RightJoin(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "right");
        }

        public Query CrossJoin(string table)
        {
            return Join(j => j.JoinWith(table).AsCross());
        }

    }
}