using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public abstract record Q
    {
        public abstract void Render(StringBuilder sb, Renderer r);
    }
}