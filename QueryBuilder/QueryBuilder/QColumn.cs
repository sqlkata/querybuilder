using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QColumn(string Name) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, Name);
        }
    }
}