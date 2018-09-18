using System;
using System.Data;
using System.Linq;
using SqlKata.Compilers;
using SqlKata.Execution.Interfaces;
using SqlKata.Interfaces;

namespace SqlKata.Execution
{
    public class QueryFactory: IQueryFactory
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger { get; set; } = result => { };
        public int QueryTimeout { get; set; } = 30;

        public QueryFactory() { }

        public QueryFactory(IDbConnection connection, Compiler compiler)
        {
            Connection = connection;
            Compiler = compiler;
        }

        public IQuery Query()
        {
            var query = new XQuery(this.Connection, this.Compiler);

            query.Logger = Logger;

            return query;
        }

        public IQuery Query(string table)
        {
            return Query().From(table);
        }

        public IQuery FromQuery(IQuery query)
        {
            var xQuery = new XQuery(this.Connection, this.Compiler);

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