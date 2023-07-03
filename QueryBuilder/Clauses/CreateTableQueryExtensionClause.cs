using SqlKata.Contract.CreateTable.DbTableSpecific;

namespace SqlKata.Clauses
{
    /// <summary>
    /// this clause is for other queries that are supported by specific dbs. for example in oracle we have ON COMMIT PRESERVER ROWS for creating temp table
    /// and we do not have this option in other databases like mysql!
    /// this can be inherited and used for other dbs.
    /// </summary>
    public class CreateTableQueryExtensionClause : AbstractClause
    {
        public CreateDbTableExtension CreateDbTableExtension { get; set; }

        public override AbstractClause Clone()
        {
            return new CreateTableQueryExtensionClause()
            {
                Component = Component,
                Engine = Engine,
                CreateDbTableExtension = CreateDbTableExtension
            };
        }
    }
}
