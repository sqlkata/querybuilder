using SqlKata.Compilers;
using System.Text;

namespace SqlKata
{
    public abstract class AbstractClause
    {
        public required string? Engine { get; init; }
        public required string Component { get; init; }
        public virtual void Render(StringBuilder sb, Renderer r){}
    }
}
