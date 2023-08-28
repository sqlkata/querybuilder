using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public static class QExt
    {
        public static string Render(this Q q, Renderer renderer)
        {
            var sb = new StringBuilder();
            q.Render(sb, renderer);
            return sb.ToString();
        }
    }
}
