using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public static class QExt
    {
        public static string Render(this Q q, BindingMode bindingMode)
        {
            var sb = new StringBuilder();
            q.Render(sb, new Renderer(new X("[", "]", "AS "))
            {
                BindingMode = bindingMode
            });
            return sb.ToString();
        }
    }
}