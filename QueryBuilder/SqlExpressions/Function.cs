using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlKata.SqlExpressions
{
    public class Function : AbstractSqlExpression
    {
        public string Name { get; }
        public List<Expression> Values { get; }

        public Function(string name, params string[] args)
        {
            this.Name = name;
            Values = args.Select(x => new Identifier(x)).Cast<Expression>().ToList();
        }

        public Function(string name, params Expression[] expressions)
        {
            this.Name = name;
            Values = expressions.ToList();
        }

    }
}