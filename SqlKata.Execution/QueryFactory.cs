using System;
using System.Data;
using System.Linq;
using SqlKata;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class QueryFactory
    {
        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger = result => { };
        public int QueryTimeout { get; set; } = 30;

        public QueryFactory() { }

        public QueryFactory(IDbConnection connection, Compiler compiler, IDbTransaction transaction = null)
        {
            Connection = connection;
            Compiler = compiler;
            Transaction = transaction;
        }

        public Query Query()
        {
            var query = new XQuery(this.Connection, this.Compiler, this.Transaction);

            query.Logger = Logger;

            return query;
        }

        public Query Query(string table)
        {
            return Query().From(table);
        }

        public Query FromQuery(Query query)
        {
            var xQuery = new XQuery(this.Connection, this.Compiler, this.Transaction);

            xQuery.Clauses = query.Clauses.Select(x => x.Clone()).ToList();

            xQuery.QueryAlias = query.QueryAlias;
            xQuery.IsDistinct = query.IsDistinct;
            xQuery.Method = query.Method;

            xQuery.SetEngineScope(query.EngineScope);

            xQuery.Logger = Logger;

            return xQuery;
        }

    }
}