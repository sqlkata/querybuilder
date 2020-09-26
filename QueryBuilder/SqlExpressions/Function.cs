using System.Collections.Generic;
using System.Linq;

namespace SqlKata.SqlExpressions
{
    public class Function : SqlExpression, HasBinding
    {
        public string Name { get; }
        public List<SqlExpression> Values { get; }

        public Function(string name, params string[] args)
        {
            this.Name = name;
            Values = args.Select(x => new Identifier(x)).Cast<SqlExpression>().ToList();
        }

        public Function(string name, params SqlExpression[] expressions)
        {
            this.Name = name;
            Values = expressions.ToList();
        }

        public IEnumerable<object> GetBindings()
        {
            var values = new List<object>();

            foreach (var value in Values)
            {
                if (value is HasBinding withBinding)
                {
                    values.AddRange(withBinding.GetBindings());
                }
            }

            return values;
        }
    }
}