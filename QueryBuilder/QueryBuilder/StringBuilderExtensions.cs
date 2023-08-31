using System.Text;

namespace SqlKata
{
    public static class StringBuilderExtensions{
        
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

            if (any) sb.Remove(sb.Length - separator.Length, separator.Length);
        }
    }
}
