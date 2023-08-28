using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public record QInsertQuery(InsertQueryClause Iqc) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            throw new NotImplementedException();
        }
    }
}