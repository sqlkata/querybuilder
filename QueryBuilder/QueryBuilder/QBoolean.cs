using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QBoolean(bool Value) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Value ? r.True : r.False);
        }
    }
}