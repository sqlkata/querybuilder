using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QDialect(Dictionary<Dialect, Q> Branches) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Branches.TryGetValue(r.Dialect, out var expression))
                expression.Render(sb, r);
            else if (Branches.TryGetValue(Dialect.None, out expression))
                expression.Render(sb, r);
        }
    }
}