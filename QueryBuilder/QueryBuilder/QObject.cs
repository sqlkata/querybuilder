using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QObject(object? Value) : QParameter
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            switch (r.BindingMode)
            {
                case BindingMode.Placeholders:
                    sb.Append(r.ParameterPlaceholder);
                    break;
                case BindingMode.Params:
                    sb.Append(r.ParameterPrefix);
                    sb.Append(r.NextParameter());
                    break;
                case BindingMode.Values:
                    sb.RenderSqlValue(Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
