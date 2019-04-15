using System;
using System.Data;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class QueryFactory
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger = result => { };
        public int QueryTimeout { get; set; } = 30;

        public QueryFactory() { }

        public QueryFactory(IDbConnection connection, Compiler compiler)
        {
            Connection = connection;
            Compiler = compiler;
        }

        public Query Query()
        {
            var query = new XQuery(this.Connection, this.Compiler);

            query.QueryFactory = this;

            query.Logger = Logger;

            return query;
        }

        public Query Query(string table)
        {
            return Query().From(table);
        }

        /// <summary>
        /// Create an XQuery instance from a regular Query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query FromQuery(Query query)
        {
            var xQuery = new XQuery(this.Connection, this.Compiler);

            xQuery.QueryFactory = this;

            xQuery.Clauses = query.Clauses.Select(x => x.Clone()).ToList();

            xQuery.QueryAlias = query.QueryAlias;
            xQuery.IsDistinct = query.IsDistinct;
            xQuery.Method = query.Method;
            xQuery.Includes = query.Includes;

            xQuery.SetEngineScope(query.EngineScope);

            xQuery.Logger = Logger;

            return xQuery;
        }

        /// <summary>
        /// Compile and log query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        internal SqlResult Compile(Query query)
        {
            var compiled = this.Compiler.Compile(query);

            this.Logger(compiled);

            return compiled;
        }

    }
}