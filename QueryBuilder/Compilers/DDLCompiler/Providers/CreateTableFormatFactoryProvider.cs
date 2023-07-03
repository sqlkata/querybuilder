using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Providers
{
    internal class CreateTableFormatFactoryProvider : ICreateTableFormatFactoryProvider
    {
        private readonly Dictionary<DataSource, ICreateQueryFormatFactory> _createQueryFormatFactoriesByDataSource;

        public CreateTableFormatFactoryProvider(IEnumerable<ICreateQueryFormatFactory> createQueryFormatFactories)
        {
            _createQueryFormatFactoriesByDataSource = createQueryFormatFactories.ToDictionary(x => x.DataSource);
        }

        public ICreateQueryFormatFactory GetCreateQueryFormatFactory(DataSource dataSource)
        {
            return _createQueryFormatFactoriesByDataSource[dataSource];
        }
    }
}
