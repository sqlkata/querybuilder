using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.DDLCompiler.Providers
{
    internal class SqlCommandUtilProvider : ISqlCreateCommandProvider
    {
        private readonly Dictionary<DataSource, ISqlCreateCommandUtil> _sqlCreateCommandUtilsByDataSource;
        public SqlCommandUtilProvider(IEnumerable<ISqlCreateCommandUtil> sqlCreateCommandUtils)
        {
            _sqlCreateCommandUtilsByDataSource = sqlCreateCommandUtils.ToDictionary(x => x.DataSource);
        }


        public ISqlCreateCommandUtil GetSqlCreateCommandUtil(DataSource dataSource)
        {
            return _sqlCreateCommandUtilsByDataSource[dataSource];
        }
    }
}
