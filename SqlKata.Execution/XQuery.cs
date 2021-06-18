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
        public Action<SqlResult> Logger = result => { };
        public QueryFactory QueryFactory { get; set; }

        public XQuery(IDbConnection connection, Compiler compiler)
        {
            this.Connection = connection;
            this.Compiler = compiler;
        }

        public override Query Clone()
        {

            var query = new XQuery(this.Connection, this.Compiler);

            query.Clauses = this.Clauses.Select(x => x.Clone()).ToList();
            query.Logger = this.Logger;

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
