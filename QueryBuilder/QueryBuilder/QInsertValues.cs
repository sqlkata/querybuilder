using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QLiteral(string Literal) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Literal);
        }
    }

    public sealed record QNullCondition(bool IsNot) : QCondition
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(IsNot ? "IS NOT NULL" : "IS NULL");
        }
    }
    public sealed record QBoolean(bool Value) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Value ? r.True : r.False);
        }
    }
    public sealed record QOperator(string Operator) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(r.Operators.CheckOperator(Operator));
        }
    }
    public sealed record QRoundBraces(Q Exp) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("(");
            Exp.Render(sb, r);
            sb.Append(")");
        }
    }
    public sealed record QColumn(string Name) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.Wrap(sb, Name);
        }
    }
    public sealed record QAsAlias(string Name) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            r.X.AsAlias(sb, Name);
        }
    }

    public abstract record QTableExpression : Q;
    public abstract record QCondition : Q;
    public record QConditionTag(bool? IsOr, Q Condition) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (IsOr is true)
            {
                sb.Append("OR ");
            }
            else if (IsOr is false)
            {
                sb.Append("AND ");
            }
            Condition.Render(sb, r);
        }
    }

    public record QCondHeader(bool Show, string Header, Q Expression) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Show)
            {
                sb.Append(Header);
                sb.Append(" ");
            }

            Expression.Render(sb, r);
        }
    }

    public record QPrefix(string Header, Q Expression) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Header);
            Expression.Render(sb, r);
        }
    }
    public record QHeader(string Header, Q Expression) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(Header);
            sb.Append(" ");
            Expression.Render(sb, r);
        }
    }

    public record QNot(bool IsNot, Q Condition) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
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
