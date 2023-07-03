using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler
{
    internal class DDLCompiler : IDDLCompiler
    {
        private readonly ICreateTableQueryCompiler _createTableQueryCompiler;

        public DDLCompiler(ICreateTableQueryCompiler createTableQueryCompiler)
        {
            _createTableQueryCompiler = createTableQueryCompiler;
        }


        public SqlResult CompileCreateTable(Query query,DataSource dataSource)
        {
            var result = new SqlResult()
            {
                Query = query.Clone(),
            };
            var queryString = _createTableQueryCompiler.CompileCreateTable(result.Query,dataSource);
            result.RawSql = queryString;
            return result;
        }

    }
}
