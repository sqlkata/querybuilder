using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QClause(AbstractClause Clause) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            Clause.Render(sb, r);

        }
    }

    public sealed record QInsertStartClause(bool IsMultiValue) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(IsMultiValue
                ? r.MultiInsertStartClause
                : r.SingleInsertStartClause);
        }
    }

    public sealed record QCondTrailer(Q Expression, bool Condition, Q Trailer) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            Expression.Render(sb, r);
            if (Condition)
            {
                Trailer.Render(sb, r);
            }
        }
    }

    public sealed record QLastId : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(";");
            sb.Append(r.LastId);
        }
    }

    public sealed record QDialect(Dictionary<Dialect, Q> Branches) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            if (Branches.TryGetValue(r.Dialect, out var expression))
                expression.Render(sb, r);
            else if (Branches.TryGetValue(Dialect.None, out expression))
                expression.Render(sb, r);
        }
    }

    public sealed record QSingleValueInsert(
        QInsertValues Value) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append("VALUES ");
            sb.Append("(");
            Value.Render(sb, r);
            sb.Append(")");
        }
    }
}
