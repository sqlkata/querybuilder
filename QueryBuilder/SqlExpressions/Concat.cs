using System.Collections.Generic;
using System.Linq;

namespace SqlKata.SqlExpressions
{
    public class Concat : AbstractSqlExpression
    {
        public List<AbstractSqlExpression> Values { get; }

        public Concat(params string[] parameters)
        {
            Values = parameters.Select(x => new Identifier(x)).Cast<AbstractSqlExpression>().ToList();
        }

        public Concat(params AbstractSqlExpression[] values)
        {
            Values = values.ToList();
        }

    }
}