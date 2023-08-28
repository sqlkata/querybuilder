using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QTailMultiValueInsert(QInsertValues[] Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {

            // we are continuing comma separated list
            sb.Remove(sb.Length - 1, 1);

            sb.Append(", ");
            sb.RenderList(", ", Values, v =>
            {
                sb.Append("(");
                v.Render(sb, r);
                sb.Append(")");
            });
        }
    }
    public sealed record QTailMultiValueInsertFirebird(QInsertValues[] Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {

            sb.RenderList(" ", Values, v =>
            {
                sb.Append("FROM RDB$DATABASE UNION ALL SELECT ");
                v.Render(sb, r);
            });

            sb.Append(" FROM RDB$DATABASE");
        }
    }
    public sealed record QTailMultiValueInsertOracle(
        AbstractFrom From,
        QInsertColumns Columns,
        QInsertValues[] Values) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            sb.RenderList(" ", Values, v =>
            {
                sb.Append("INTO ");
                From.Render(sb, r);
                sb.Append(" ");
                Columns.Render(sb, r);
                sb.Append(" VALUES ");
                sb.Append("(");
                v.Render(sb, r);
                sb.Append(")");
            });
            sb.Append(" ");
            sb.Append("SELECT 1 FROM DUAL");
        }
    }
}
