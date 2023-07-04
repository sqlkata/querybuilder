using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler
{
    internal class DDLCompiler : IDDLCompiler
    {
        private readonly ICreateTableQueryCompiler _createTableQueryCompiler;
        private readonly ICreateTableAsCompiler _createTableAsCompiler;
        private readonly IDropTableQueryFactory _dropTableQueryFactory;
        private readonly ITruncateTableQueryFactory _truncateTableQueryFactory;

        public DDLCompiler(ICreateTableQueryCompiler createTableQueryCompiler,
            ICreateTableAsCompiler createTableAsCompiler, ITruncateTableQueryFactory truncateTableQueryFactory,
            IDropTableQueryFactory dropTableQueryFactory)
        {
            _createTableQueryCompiler = createTableQueryCompiler;
            _createTableAsCompiler = createTableAsCompiler;
            _truncateTableQueryFactory = truncateTableQueryFactory;
            _dropTableQueryFactory = dropTableQueryFactory;
        }


        public SqlResult CompileCreateTable(Query query, DataSource dataSource)
        {
            var result = new SqlResult()
            {
                Query = query.Clone(),
                RawSql = _createTableQueryCompiler.CompileCreateTable(query, dataSource)
            };
            return result;
        }

        public SqlResult CompileCreateTableAs(Query query, DataSource dataSource, string compiledSelectQuery)
        {
            var result = new SqlResult
            {
                Query = query.Clone(),
                RawSql = _createTableAsCompiler.CompileCreateAsQuery(query, dataSource, compiledSelectQuery)
            };
            return result;
        }

        public SqlResult CompileDropTable(Query query)
        {
            var result = new SqlResult
            {
                Query = query.Clone(),
                RawSql = _dropTableQueryFactory.CompileQuery(query)
            };

            return result;
        }

        public SqlResult CompileTruncateTable(Query query)
        {
            var result = new SqlResult
            {
                Query = query.Clone(),
                RawSql = _truncateTableQueryFactory.CompileQuery(query)
            };
            return result;
        }
    }
}
