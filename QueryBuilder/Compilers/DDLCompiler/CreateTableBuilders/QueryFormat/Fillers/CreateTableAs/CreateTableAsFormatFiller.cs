using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers.CreateTableAs
{
    internal class CreateTableAsFormatFiller : ICreateTableAsFormatFiller
    {
        private readonly ISqlCreateCommandProvider _sqlCreateCommandProvider;
        private readonly IOracleCreateTableDbExtender _oracleCreateTableDbExtender;


        public CreateTableAsFormatFiller(ISqlCreateCommandProvider sqlCreateCommandProvider, IOracleCreateTableDbExtender oracleCreateTableDbExtender)
        {
            _sqlCreateCommandProvider = sqlCreateCommandProvider;
            _oracleCreateTableDbExtender = oracleCreateTableDbExtender;
        }


        public string FillCreateTableAsQuery(string queryFormat,string compiledSelectQuery, Query query,DataSource dataSource)
        {
            var tableName = query.GetOneComponent<FromClause>("from").Table;
            var tableType = query.GetOneComponent<TableCluase>("TableType").TableType;
            var tempTableClause = tableType == TableType.Temporary ? _sqlCreateCommandProvider.GetSqlCreateCommandUtil(dataSource).GetTempTableClause() : "";
            var isOracleTempTable = dataSource == DataSource.Oracle && tableType == TableType.Temporary;
            var tableExtensions = query
                .GetOneComponent<CreateTableQueryExtensionClause>("CreateTableExtension");
            var onCommitBehaviour = isOracleTempTable ? _oracleCreateTableDbExtender.GetOnCommitBehaviour(tableExtensions) : "";
            return string.Format(queryFormat,
                tempTableClause,
                tableName,
                onCommitBehaviour,
                compiledSelectQuery);
        }
    }
}
