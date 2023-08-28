using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QInsertStartClause(bool IsMultiValue) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(IsMultiValue
                ? r.MultiInsertStartClause
                : r.SingleInsertStartClause);
        }
    }
}