using System.Data;
using System.Linq;
using SqlKata;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class QueryFactory
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }

        public QueryFactory() { }

        public QueryFactory(IDbConnection connection, Compiler compiler)
        {
            Connection = connection;
            Compiler = compiler;
        }

        public Query Query()
        {
            return new XQuery(this.Connection, this.Compiler);
        }

        public Query Query(string table)
        {
            return Query().From(table);
        }

        public Query Query(Query query)
        {
            var xQuery = new XQuery(this.Connection, this.Compiler);

            xQuery.Clauses = query.Clauses.Select(x => x.Clone()).ToList();

            xQuery.QueryAlias = query.QueryAlias;
            xQuery.IsDistinct = query.IsDistinct;
            xQuery.Method = query.Method;

            xQuery.SetEngineScope(query.EngineScope);

            return xQuery;
        }

    }
}