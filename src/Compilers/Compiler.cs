using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{
    public partial class Compiler : AbstractCompiler
    {
        private string[] selectComponents = new string[] {
            "aggregate",
            "columns",
            "from",
            "joins",
            "wheres",
            "groups",
            "havings",
            "orders",
            "limit",
            "offset",
            "unions",
            "lock",
        };

        public virtual Query OnBeforeCompile(Query query)
        {
            return query;
        }

        public virtual string OnAfterCompile(string sql, List<object> bindings)
        {
            return sql;
        }

        public virtual string CompileSelect(Query query)
        {
            if (!query.Has("select"))
            {
                query.Select("*");
            }

            var results = CompileComponents(query);

            return JoinComponents(results, "select");
        }

        protected List<string> CompileComponents(Query query)
        {
            var result = (new List<string>
            {
                this.CompileAggregate(query),
                this.CompileColumns(query),
                this.CompileFrom(query),
                this.CompileJoins(query),
                this.CompileWheres(query),
                this.CompileGroups(query),
                this.CompileHavings(query),
                this.CompileOrders(query),
                this.CompileLimit(query),
                this.CompileOffset(query),
                this.CompileUnions(query),
                this.CompileLock(query),
            })
            .ToList()
            .Where(x => x != null)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

            return result;
        }


        protected virtual string CompileColumns(Query query)
        {
            // If the query is actually performing an aggregating select, we will let that
            // compiler handle the building of the select clauses, as it will need some
            // more syntax that is best handled by that function to keep things neat.
            if (query.Has("aggregate"))
            {
                return null;
            }

            if (!query.Has("select"))
            {
                return null;
            }

            var columns = query.Get("select").Cast<AbstractColumn>().ToList();

            var select = (query._Distinct ? "SELECT DISTINCT " : "SELECT ");
            return select + (columns.Any() ? Columnize(columns) : "*");
        }

        protected virtual string CompileAggregate(Query query)
        {

            if (!query.Has("aggregate"))
            {
                return null;
            }

            var ag = query.Get("aggregate").Cast<AggregateClause>().First();

            var cols = ag.Columns
                .Select(x => new Column { Name = x })
                .Cast<AbstractColumn>()
                .ToList();

            var columns = Columnize(cols);

            if (query._Distinct && columns != "*")
            {
                columns = "DISTINCT " + columns;
            }

            return "SELECT " + ag.Type.ToUpper() + "(" + columns + ") AS " + Wrap("count");
        }

        protected virtual string CompileTableExpression(AbstractFrom from)
        {
            if (from is RawFrom)
            {
                return (from as RawFrom).Expression;
            }

            if (from is QueryFrom)
            {
                var fromQuery = (from as QueryFrom).Query;

                var alias = string.IsNullOrEmpty(fromQuery._Alias) ? "" : " AS " + WrapValue(fromQuery._Alias);

                var compiled = CompileSelect(fromQuery);

                return "(" + separator + compiled + separator + ")" + alias;
            }

            if (from is From)
            {
                return WrapTable((from as From).Table);
            }

            throw InvalidClauseException("TableExpression", from);
        }

        protected virtual string CompileFrom(Query query)
        {
            if (!query.Has("from"))
            {
                return null;
            }

            var from = query.GetOne<AbstractFrom>("from");

            return "FROM " + CompileTableExpression(from);
        }

        protected virtual string CompileJoins(Query query)
        {
            if (!query.Has("join"))
            {
                return null;
            }

            var joins = query.Get<BaseJoin>("join");

            var sql = new List<string>();

            foreach (var item in joins)
            {
                sql.Add(compileJoin(item.Join));
            }

            return JoinComponents(sql, "join");
        }

        protected virtual string compileJoin(Join join, bool isNested = false)
        {

            var from = join.GetOne<AbstractFrom>("from");

            var joinTable = CompileTableExpression(from);


            // Compile constraints 
            var wheres = new List<string>();

            var constraints = join.Get<AbstractCondition>("where");

            for (var i = 0; i < constraints.Count; i++)
            {
                var compiled = CompileCondition(constraints[i]);

                if (string.IsNullOrEmpty(compiled))
                {
                    break;
                }

                var boolOperator = i == 0 ? "" : constraints[i].IsOr ? "OR " : "AND ";

                wheres.Add(boolOperator + compiled);
            }

            var onClause = constraints.Any() ? "ON " + string.Join(" ", wheres) : "";

            return $"{join.Type} JOIN {joinTable} {onClause}";
        }



        protected virtual string CompileCondition(AbstractCondition clause)
        {
            var name = clause.GetType().Name;
            name = name.Substring(0, name.IndexOf("Condition"));

            var methodName = "Compile" + name + "Condition";
            return dynamicCompile(methodName, clause);
        }

        protected virtual string CompileWheres(Query query)
        {
            if (!query.Has("from") || !query.Has("where"))
            {
                return null;
            }

            var conditions = query.Get<AbstractCondition>("where");
            var sql = CompileConditions(conditions);

            return $"WHERE {sql}";
        }

        protected string CompileQuery<T>(
                BaseQuery<T> query,
                string joinType = "",
                bool isNested = false
        ) where T : BaseQuery<T>
        {
            if (query is Query)
            {
                return CompileSelect(query as Query);
            }

            if (query is Join)
            {
                return compileJoin((query as Join), isNested);
            }

            return "";
        }



        protected virtual string CompileSubQueryCondition<T>(QueryCondition<T> x) where T : BaseQuery<T>
        {
            var select = CompileQuery(x.Query);
            var alias = string.IsNullOrEmpty(x.Query._Alias) ? "" : " AS " + x.Query._Alias;
            return Wrap(x.Column) + " " + x.Operator + " (" + select + ")" + alias;
        }

        protected virtual string CompileGroups(Query query)
        {
            if (!query.Has("group"))
            {
                return null;
            }

            var cols = query.Get("group")
                .Select(x => x as AbstractColumn)
                .ToList();

            return "GROUP BY " + Columnize(cols);
        }

        protected virtual string CompileOrders(Query query)
        {
            if (!query.Has("order"))
            {
                return null;
            }

            var columns = query.Get<AbstractOrderBy>("order").Select(x =>
            {

                if (x is RawOrderBy)
                {
                    return (x as RawOrderBy).Expression;
                }

                var direction = (x as OrderBy).Ascending ? "ASC" : "DESC";

                return Wrap((x as OrderBy).Column) + " " + direction;
            });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public string CompileHavings(Query query)
        {
            if (!query.Has("having"))
            {
                return null;
            }

            var sql = new List<string>();
            string boolOperator;

            var havings = query.Get("having")
                .Cast<AbstractCondition>()
                .ToList();

            for (var i = 0; i < havings.Count; i++)
            {
                var compiled = CompileCondition(havings[i]);

                if (!string.IsNullOrEmpty(compiled))
                {
                    boolOperator = i > 0 ? havings[i].IsOr ? "OR " : "AND " : "";

                    sql.Add(boolOperator + "HAVING " + compiled);
                }
            }

            return JoinComponents(sql, "having");
        }

        protected virtual string CompileLimit(Query query)
        {
            var limitOffset = query.GetOne("limit") as LimitOffset;

            if (limitOffset != null && limitOffset.HasLimit())
            {
                return "LIMIT ?";
            }

            return "";
        }

        protected virtual string CompileOffset(Query query)
        {
            var limitOffset = query.GetOne("limit") as LimitOffset;

            if (limitOffset != null && limitOffset.HasOffset())
            {
                return "OFFSET ?";
            }

            return "";
        }

        protected virtual string CompileUnions(Query query)
        {
            // throw new NotImplementedException();
            return null;
        }

        protected virtual string CompileLock(Query query)
        {
            // throw new NotImplementedException();
            return null;
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

        private InvalidCastException InvalidClauseException(string section, AbstractClause clause)
        {
            return new InvalidCastException($"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");
        }

        private string Capitalize(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }

        protected string dynamicCompile(string name, AbstractClause clause)
        {

            MethodInfo methodInfo = this.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception($"Failed to locate a compiler for {name}.");
            }

            if (methodInfo.GetGenericArguments().Any() && clause.GetType().GetTypeInfo().IsGenericType)
            {
                methodInfo = methodInfo.MakeGenericMethod(clause.GetType().GenericTypeArguments.First());
            }

            var result = methodInfo.Invoke(this, new object[] { clause });

            return result as string;
        }
    }

}