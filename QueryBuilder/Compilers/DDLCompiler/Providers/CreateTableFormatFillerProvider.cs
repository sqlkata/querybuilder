using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Providers
{
    internal class CreateTableFormatFillerProvider : ICreateTableQueryFillerProvider
    {
        private readonly Dictionary<DataSource, ICreateQueryFormatFiller> _createQueryFormatFillersByDataSource;

        public CreateTableFormatFillerProvider(IEnumerable<ICreateQueryFormatFiller> createQueryFormatFillers)
        {
            _createQueryFormatFillersByDataSource = createQueryFormatFillers.ToDictionary(x => x.DataSource);
        }

        public ICreateQueryFormatFiller GetCreateQueryFormatFiller(DataSource dataSource)
        {
            return _createQueryFormatFillersByDataSource[dataSource];
        }
    }
}
