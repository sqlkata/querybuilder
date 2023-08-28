using System.Collections.Immutable;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed class QueryBuilder
    {
        private readonly Query _query;

        public QueryBuilder(Query query)
        {
            _query = query;
        }

        public Q Build()
        {
            if (_query.Method == "insert")
            {
                return CompileInsertQuery();
            }

            throw new NotImplementedException();
        }

        private Q CompileInsertQuery()
        {
            var from = _query.Components.GetOneComponent<AbstractFrom>("from");
            if (from is null)
                throw new InvalidOperationException("No table set to insert");

            if (from is not FromClause and not RawFromClause)
                throw new InvalidOperationException("Invalid table expression");

            
            var inserts = _query.Components.GetComponents<AbstractInsertClause>("insert");
            if (inserts[0] is InsertQueryClause iqc)
                return new QInsertQuery(iqc);

            var first = (InsertClause)inserts[0];
            var columns = new QInsertColumns(first.Columns);
            var values = inserts
                .Cast<InsertClause>()
                .Select(c => new QInsertValues(
                    c.Values.Select(Parametrize).ToImmutableArray()))
                .ToArray();
            var returnId = first.ReturnId;
            return new QValueInsert(from, columns,values, returnId);
        }

        private static QParameter Parametrize(object? parameter)
        {
            return parameter switch
            {
                UnsafeLiteral literal => new QUnsafeLiteral(literal),
                Variable variable => new QVariable(variable),
                _ => new QObject(parameter)
            };
        }
    }

    public record QInsertQuery(InsertQueryClause Iqc) : Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            throw new NotImplementedException();
        }
    }
}
