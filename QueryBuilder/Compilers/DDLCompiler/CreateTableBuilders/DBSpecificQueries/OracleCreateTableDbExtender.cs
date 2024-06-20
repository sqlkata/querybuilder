using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Contract.CreateTable.DbTableSpecific;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.DBSpecificQueries
{
    public class OracleCreateTableDbExtender : IOracleCreateTableDbExtender
    {
        public string GetOnCommitBehaviour(CreateTableQueryExtensionClause createTableQueryExtensionClause)
        {
            if (createTableQueryExtensionClause == null)
                return "on commit delete rows";
            var commitPreserveRows = createTableQueryExtensionClause.CreateDbTableExtension != null && ((OracleDbTableExtensions)createTableQueryExtensionClause.CreateDbTableExtension)
                .OnCommitPreserveRows;
            return commitPreserveRows ? "on commit preserve rows" : "on commit delete rows";
        }
    }
}
