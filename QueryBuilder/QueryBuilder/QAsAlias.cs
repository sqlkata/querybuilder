using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QAsAlias(string Name) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.AsAlias(sb, Name);
        }
    }
}