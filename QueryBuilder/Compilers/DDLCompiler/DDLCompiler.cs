using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler
{
    internal class DDLCompiler
    {
        private readonly ICreateTableQueryCompiler _createTableQueryCompiler;

        public DDLCompiler(ICreateTableQueryCompiler createTableQueryCompiler)
        {
            _createTableQueryCompiler = createTableQueryCompiler;
        }


        internal SqlResult CompileCreateTable(Query query)
        {
            var result = new SqlResult()
            {
                Query = query.Clone(),
            };
            var queryString = _createTableQueryCompiler.CompileCreateTable(result.Query);
            result.RawSql = queryString;
            return result;
        }

    }
}
