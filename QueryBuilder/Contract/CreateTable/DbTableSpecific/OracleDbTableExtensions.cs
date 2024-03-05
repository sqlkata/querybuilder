namespace SqlKata.Contract.CreateTable.DbTableSpecific
{
    public class OracleDbTableExtensions : CreateDbTableExtension
    {
        public bool OnCommitPreserveRows { get; set; }
        public string hint { get; set; }
    }
}
