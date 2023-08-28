using System.Collections.Immutable;

namespace SqlKata
{
    public sealed class QueryBuilder
    {
        public required string Method { get; init; }
        public required IComponentList Components { get; init; }

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
            var fromClause = Components.GetOneComponent<AbstractFrom>("from");
            if (fromClause is null)
                throw new InvalidOperationException("No table set to insert");
            var inserts = Components.GetComponents<AbstractInsertClause>("insert");
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
}
