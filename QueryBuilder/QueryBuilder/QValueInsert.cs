using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QInsertColumns(ImmutableArray<string> Names) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.Append(" (");
            sb.RenderList(", ", Names, c => r.X.Wrap(sb, c));
            sb.Append(")");
        }
    }

    public sealed record QValueInsert(
        AbstractFrom From,
        QInsertColumns Columns,
        QInsertValues[] Values,
        bool ReturnId) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            var isMultiValueInsert = Values.Length > 1;
            sb.Append(isMultiValueInsert ? r.MultiInsertStartClause : r.SingleInsertStartClause);
            sb.Append(" ");
            From.Render(sb, r);
            Columns.Render(sb, r);
            if (r.Dialect == Dialect.Firebird && isMultiValueInsert)
            {
                sb.Append(" SELECT ");
                Values.First().Render(sb, r);
            }
            else
            {
                sb.Append(" VALUES ");
                sb.Append("(");
                Values.First().Render(sb, r);
                sb.Append(")");
            }

            if (isMultiValueInsert)
            {
                if (r.Dialect == Dialect.Oracle)
                {
                    sb.RenderList("", Values.Skip(1), v =>
                    {
                        sb.Append(" INTO ");
                        From.Render(sb, r);
                        Columns.Render(sb, r);
                        sb.Append(" VALUES ");
                        sb.Append("(");
                        v.Render(sb, r);
                        sb.Append(")");
                    });
                    sb.Append(" SELECT 1 FROM DUAL");
                }
                else if (r.Dialect == Dialect.Firebird)
                {
                    sb.RenderList("", Values.Skip(1), v =>
                    {
                        sb.Append(" FROM RDB$DATABASE UNION ALL SELECT ");
                        v.Render(sb, r);
                    });

                    sb.Append(" FROM RDB$DATABASE");

                }
                else
                {
                    sb.Append(", ");
                    sb.RenderList(", ", Values.Skip(1), v =>
                    {
                        sb.Append("(");
                        v.Render(sb, r);
                        sb.Append(")");
                    });
                }
            }
            if (ReturnId)
            {
                sb.Append(";");
                sb.Append(r.LastId);
            }
        }
    }
}
