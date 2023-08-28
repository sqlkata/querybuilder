using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QFromClause(FromClause From) : QTableExpression
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, From.Table);
        }
    }
}