using System;
using System.Data;
using System.Linq;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution.Interfaces;
using SqlKata.Interfaces;

namespace SqlKata.Execution
{
    public class XQuery: Query, IXQuery
    {
        public IDbConnection Connection { get; set; }
        public Compiler Compiler { get; set; }
        public Action<SqlResult> Logger { get; set; } = result => { };

        public XQuery(IDbConnection connection, Compiler compiler)
        {
            this.Connection = connection;
            this.Compiler = compiler;
        }

        public override IQuery Clone()
        {

            var query = new XQuery(this.Connection, this.Compiler);

            query.Clauses = this.Clauses.Select(x => x.Clone()).ToList();
            query.Logger = this.Logger;

            query.QueryAlias = QueryAlias;
            query.IsDistinct = IsDistinct;
            query.Method = Method;

            query.SetEngineScope(EngineScope);

            return query;

        }

    }

}
