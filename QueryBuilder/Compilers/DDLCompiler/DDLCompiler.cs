using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler
{
    internal class DDLCompiler : IDDLCompiler
    {
        private readonly ICreateTableQueryCompiler _createTableQueryCompiler;
        private readonly ICreateTableAsCompiler _createTableAsCompiler;

        public DDLCompiler(ICreateTableQueryCompiler createTableQueryCompiler, ICreateTableAsCompiler createTableAsCompiler)
        {
            _createTableQueryCompiler = createTableQueryCompiler;
            _createTableAsCompiler = createTableAsCompiler;
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

        public SqlResult CompileCreateTableAs(Query query, DataSource dataSource,string compiledSelectQuery)
        {
            var result = new SqlResult()
            {
                Query = query.Clone()
            };
            var queryString = _createTableAsCompiler.CompileCreateAsQuery(query,dataSource,compiledSelectQuery);
            result.RawSql = queryString;
            return result;
        }
    }
}
