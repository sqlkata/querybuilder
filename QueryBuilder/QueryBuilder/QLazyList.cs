using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QLazyList<T>(string Separator,
        ICollection<T> Source, Func<T, Q?> Selector) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.RenderList(Separator, Source.Select(Selector).OfType<Q>(), r);

        }
    }
}