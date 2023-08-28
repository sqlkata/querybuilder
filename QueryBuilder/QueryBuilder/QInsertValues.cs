using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QNullCondition(bool IsNot) : QCondition
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(IsNot ? "IS NOT NULL" : "IS NULL");
        }
    }
    public sealed record QOperator(string Operator) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.Operators.CheckOperator(Operator));
        }
    }
    public sealed record QColumn(string Name) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, Name);
        }
    }

    public abstract record QTableExpression : Q;
    public abstract record QCondition : Q;
    public record QConditionTag(bool? IsOr, bool IsNot, Q Condition) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            //var boolOperator = i == 0 ? "" : conditions[i].IsOr ? "OR " : "AND ";
            if (IsOr is true)
            {
                sb.Append("OR ");
            }
            else if (IsOr is false)
            {
                sb.Append("AND ");
            }

            if (IsNot)
            {
                sb.Append("NOT (");
            }
            Condition.Render(sb, r);
            if (IsNot)
            {
                sb.Append(")");
            }
        }
    }

    public sealed record QWhere(ImmutableArray<QConditionTag> Conditions) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("WHERE ");
            sb.RenderList(" ", Conditions, r);
        }
    }

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
