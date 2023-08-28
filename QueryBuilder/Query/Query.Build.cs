using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public partial class Query
    {
        public Q Build()
        {
            if (Method == "insert")
            {
                return CompileInsertQuery();
            }

            throw new NotImplementedException();
        }

        private QValueInsert CompileInsertQuery()
        {
            var fromClause = GetOneComponent<AbstractFrom>("from");
            if (fromClause is null)
                throw new InvalidOperationException("No table set to insert");
            var inserts = GetComponents<AbstractInsertClause>("insert");
            if (inserts[0] is InsertQueryClause)
                throw new NotImplementedException();

            return new QValueInsert(fromClause, inserts
                .Cast<InsertClause>()
                .Select(c => new QInsertClause(c.Columns,
                    c.Values.Select(Parametrize).ToImmutableArray()))
                .ToArray(),
                ((InsertClause)inserts[0]).ReturnId);

            static QParameter Parametrize(object? parameter)
            {
                return parameter switch
                {
                    UnsafeLiteral literal => new QUnsafeLiteral(literal),
                    Variable variable => new QVariable(variable),
                    _ => new QObject(parameter)
                };
            }


        }
    }

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
    public abstract record Q
    {
        public abstract void Render(StringBuilder sb, Renderer r);
    }

    public abstract record QParameter : Q;
    public sealed record QUnsafeLiteral(UnsafeLiteral Literal) : QParameter
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Literal.Value);
        }
    }

    public sealed record QVariable(Variable Variable) : QParameter
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.ParameterPlaceholder);
        }
    }

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
                    sb.Append("@p");
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

    public sealed record QInsertClause(
        ImmutableArray<string> Columns,
        ImmutableArray<QParameter> Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Columns.Length == 0) return;
            sb.Append(" (");
            sb.RenderList(", ", Columns, c => r.X.Wrap(sb, c));
            sb.Append(")");
            sb.Append(" VALUES (");
            sb.RenderList(", ", Values, r);
            sb.Append(")");

        }
    }
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

    public sealed record QValueInsert(AbstractFrom From,
        QInsertClause[] Inserts, bool ReturnId) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            var isMultiValueInsert = Inserts.Length > 1;
            var firstInsert = Inserts.First();
            sb.Append(isMultiValueInsert ? r.MultiInsertStartClause : r.SingleInsertStartClause);
            sb.Append(" ");
            From.Render(sb, r);
            firstInsert.Render(sb, r);
            if (ReturnId)
            {
                sb.Append(";");
                sb.Append(r.LastId);
            }
        }
    }
}
