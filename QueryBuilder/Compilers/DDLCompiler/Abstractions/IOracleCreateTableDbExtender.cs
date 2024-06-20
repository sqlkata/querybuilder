using SqlKata.Clauses;

namespace SqlKata.Compilers.DDLCompiler.Abstractions
{
    internal interface IOracleCreateTableDbExtender
    {
        string GetOnCommitBehaviour(CreateTableQueryExtensionClause createTableQueryExtensionClause);
    }
}
