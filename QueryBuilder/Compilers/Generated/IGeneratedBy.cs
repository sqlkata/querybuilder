using SqlKata;
namespace QueryBuilder.Compilers.Generated
{
    public interface IGeneratedBy
    {
        string CommandSqlLastId { get; set; }
        GeneratedByType GeneratedByType { get; }
        string FindWord { get; }
        void Render(SqlResult result);
        string ColumnName { get; }
    }
}
