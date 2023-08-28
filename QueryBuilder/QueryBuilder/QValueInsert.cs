using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
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