
namespace SqlKata
{
    public interface IWithQuery<Q> where Q : BaseQuery<Q>
    {
        Q Query { get; set; }
    }
}