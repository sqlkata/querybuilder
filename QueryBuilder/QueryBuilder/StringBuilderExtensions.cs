using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public static class StringBuilderExtensions{
        public static void RenderList(this StringBuilder sb,
            string separator, IEnumerable<Q> list, Renderer r)
        {
            sb.RenderList(separator, list, n => n.Render(sb, r));
        }
        public static void RenderList<T>(this StringBuilder sb,
            string separator, IEnumerable<T> list, Action<T>? renderItem = null)
        {
            renderItem ??= x => sb.Append(x);
            var any = false;
            foreach (var item in list)
            {
                renderItem(item);
                sb.Append(separator);
                any = true;
            }

            if (any) sb.Remove(sb.Length - 2, 2);
        }
    }
}