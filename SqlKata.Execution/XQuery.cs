using System;
using System.Data;
using System.Linq;
using SqlKata;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class XQuery : Query
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger = result => { };

        public XQuery(IDbConnection connection, Compiler compiler)
        {
            Connection = connection;
            Compiler = compiler;
        }

        public override Query Clone()
        {

            var query = new XQuery(Connection, Compiler);

            query.Clauses = Clauses.Select(x => x.Clone()).ToList();
            query.Logger = Logger;

            query.QueryAlias = QueryAlias;
            query.IsDistinct = IsDistinct;
            query.Method = Method;

            query.SetEngineScope(EngineScope);

            return query;

        }

    }

}
