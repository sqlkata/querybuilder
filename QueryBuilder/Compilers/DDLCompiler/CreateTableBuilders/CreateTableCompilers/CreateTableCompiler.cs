using System.Collections.Generic;
using System.Text;
using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableCompilers
{
    internal class CreateTableCompiler : ICreateTableQueryCompiler
    {
        private readonly ICreateTableFormatFactoryProvider _createQueryFormatFactoryProvider;
        private readonly ICreateTableQueryFillerProvider _createQueryFormatFillerProvider;

        public CreateTableCompiler(ICreateTableQueryFillerProvider createQueryFormatFillerProvider, ICreateTableFormatFactoryProvider createQueryFormatFactoryProvider)
        {
            _createQueryFormatFillerProvider = createQueryFormatFillerProvider;
            _createQueryFormatFactoryProvider = createQueryFormatFactoryProvider;
        }

        public string CompileCreateTable(Query query,DataSource dataSource)
        {
            var formatFactory = _createQueryFormatFactoryProvider.GetCreateQueryFormatFactory(dataSource);
            var formatFiller = _createQueryFormatFillerProvider.GetCreateQueryFormatFiller(dataSource);
            var queryFormat = formatFactory.CreateTableFormat();
            var queryString = formatFiller.FillQueryFormat(queryFormat,query);
            return RefineQueryString(queryString);
        }

        private static string RefineQueryString(string queryString)
        {
            var lastCommaChar = queryString.LastIndexOf(',');
            if(lastCommaChar != -1)
                queryString = queryString.Remove(lastCommaChar,1);
            return queryString;
        }

    }
}
