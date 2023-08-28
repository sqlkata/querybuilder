using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QColumn(Column Column) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, Column.Name);
        }
    }

    public abstract record QTableExpression : Q;
    public sealed record QFrom(QTableExpression Exp) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("FROM ");
            Exp.Render(sb, r);
        }
    }

    public sealed record QFromClause(FromClause From) : QTableExpression
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, From.Table);
        }
    }

    public sealed record QSelect(ImmutableArray<QColumn> Columns) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("SELECT ");
            if (Columns.IsEmpty)
            {
                sb.Append("*");
                return;
            }
            sb.RenderList(", ", Columns, r);
            //return $"SELECT {distinct}{select}";
        }
    }

    public sealed record QList(string Separator,
        params Q?[] Elements) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.RenderList(Separator, Elements.OfType<Q>(), r);

        }
    }
    public sealed record QInsertValues(
        ImmutableArray<QParameter> Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.RenderList(", ", Values, r);

        }
    }
}
