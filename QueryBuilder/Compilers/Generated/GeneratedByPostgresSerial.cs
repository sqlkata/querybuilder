namespace QueryBuilder.Compilers.Generated
{
    public sealed class GeneratedByPostgresSerial : GeneratedBy, IGeneratedBy
    {
        public GeneratedByPostgresSerial()
            :base("SELECT lastval();", GeneratedByType.Last, "", "")
        {
        }
    }
}
