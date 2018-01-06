using System.Data;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Execution
{
    public class XQuery : Query
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }

        public XQuery(IDbConnection connection, Compiler compiler)
        {
            this.Connection = connection;
            this.Compiler = compiler;
        }

        public override Query Clone()
        {

            var query = new XQuery(this.Connection, this.Compiler);

            query.Clauses = this.Clauses.Select(x => x.Clone()).ToList();

            query.QueryAlias = QueryAlias;
            query.IsDistinct = IsDistinct;
            query.Method = Method;

            query.SetEngineScope(EngineScope);

            return query;

        }

    }

}
