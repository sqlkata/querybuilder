using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableCompilers
{
    internal class CreateTableAsCompiler : ICreateTableAsCompiler
    {
        private readonly ICreateTableAsFormatFactory _createTableAsFormatFactory;
        private readonly ICreateTableAsFormatFiller _createTableAsFormatFiller;

        public CreateTableAsCompiler(ICreateTableAsFormatFiller createTableAsFormatFiller, ICreateTableAsFormatFactory createTableAsFormatFactory)
        {
            _createTableAsFormatFiller = createTableAsFormatFiller;
            _createTableAsFormatFactory = createTableAsFormatFactory;
        }

        public string CompileCreateAsQuery(Query query, DataSource dataSource, string compiledSelectQuery)
        {
            var createTableAsFormat = _createTableAsFormatFactory.MakeCreateTableAsFormat();
            return _createTableAsFormatFiller.FillCreateTableAsQuery(createTableAsFormat,compiledSelectQuery,query,dataSource);
        }
    }
}
