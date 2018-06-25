using System;
using System.Data;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class QueryFactory
    {
        #region Properties
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger = result => { };
        public int QueryTimeout { get; set; } = 30;
        #endregion

        public QueryFactory() { }

        public QueryFactory(IDbConnection connection, Compiler compiler)
        {
            Connection = connection;
            Compiler = compiler;
        }

        public Query Query()
        {
            var query = new XQuery(Connection, Compiler) {Logger = Logger};
            return query;
        }

        public Query Query(string table)
        {
            return Query().From(table);
        }

        public Query FromQuery(Query query)
        {
            var xQuery = new XQuery(Connection, Compiler)
            {
                Clauses = query.Clauses.Select(x => x.Clone()).ToList(),
                QueryAlias = query.QueryAlias,
                IsDistinct = query.IsDistinct,
                Method = query.Method
            };

            xQuery.SetEngineScope(query.EngineScope);
            xQuery.Logger = Logger;

            return xQuery;
        }

    }
}