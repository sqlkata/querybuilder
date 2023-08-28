using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QClause(AbstractClause Clause) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            Clause.Render(sb, r);
        }
    }
}
