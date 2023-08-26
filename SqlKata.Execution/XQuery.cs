using System;
using System.Data;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class XQuery : Query
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger = _ => { };
        public QueryFactory QueryFactory { get; set; }

        public XQuery(IDbConnection connection, Compiler compiler)
        {
            QueryFactory = new QueryFactory(connection, compiler);
            Connection = connection;
            Compiler = compiler;
        }

        public Query Clone()
        {
            var query = new XQuery(QueryFactory.Connection, QueryFactory.Compiler);

            if (QueryFactory?.QueryTimeout != null)
            {
                query.QueryFactory.QueryTimeout = QueryFactory?.QueryTimeout ?? 30;
            }

            query.Clauses = Clauses.Select(x => x.Clone()).ToList();
            query.Logger = Logger;

            query.QueryAlias = QueryAlias;
            query.IsDistinct = IsDistinct;
            query.Method = Method;
            query.Includes = Includes;
            query.Variables = Variables;

            query.SetEngineScope(EngineScope);

            return query;
        }

    }

}
