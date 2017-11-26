using System.Data;
using SqlKata.Compilers;

namespace SqlKata
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

        public Query Create()
        {
            return new Query(this.Connection, this.Compiler);
        }

        public Query Create(string table)
        {
            return new Query(this.Connection, this.Compiler, table);
        }

        public Query Query() => Create();
        public Query Query(string table) => Create(table);

    }
}