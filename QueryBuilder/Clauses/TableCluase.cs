using SqlKata.Contract.CreateTable;

namespace SqlKata.Clauses
{
    public class TableCluase : AbstractClause
    {
        public TableType TableType { get; set; }
        public override AbstractClause Clone()
        {
            return new TableCluase()
            {
                TableType = TableType,
            };
        }
    }
}
