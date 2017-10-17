namespace QueryBuilder.Compilers.Generated
{
    public sealed class GeneratedBySqlServerGuid: GeneratedBy, IGeneratedBy
    {
        public GeneratedBySqlServerGuid(string find, string column)
            :base("OUTPUT inserted.{name}", GeneratedByType.Insert, find, column)
        {
            if (string.IsNullOrEmpty(column))
                CommandSqlLastId = CommandSqlLastId.Replace(".{name}", column);
        }
    }
}
