using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QList(string Separator,
        params Q?[] Elements) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.RenderList(Separator, Elements.OfType<Q>(), r);
        }
    }
}
