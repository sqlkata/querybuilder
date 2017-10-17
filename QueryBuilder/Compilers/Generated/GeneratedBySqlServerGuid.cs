namespace QueryBuilder.Compilers.Generated
{
    public sealed class GeneratedBySqlServerGuid: GeneratedBy, IGeneratedBy
    {
        public GeneratedBySqlServerGuid(string findWord, string columnName)
            :base("OUTPUT inserted.{name}", GeneratedByType.Insert, findWord, columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                CommandSqlLastId = CommandSqlLastId.Replace(".{name}", columnName);
        }
    }
}
