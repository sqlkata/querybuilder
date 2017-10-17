using SqlKata;
namespace QueryBuilder.Compilers.Generated
{
    public interface IGeneratedBy
    {
        string CommandSqlLastId { get; set; }
        GeneratedByType GeneratedByType { get; }
        string Find { get; }
        void Merge(SqlResult result);
        string Column { get; }
    }
}
