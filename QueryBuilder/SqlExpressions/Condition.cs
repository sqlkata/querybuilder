using System.Collections.Generic;

namespace SqlKata.SqlExpressions
{
    public class Condition : SqlExpression, HasBinding
    {
        public SqlExpression Column { get; }
        public string Operator { get; }
        public SqlExpression Value { get; }

        public Condition(SqlExpression column, string op, SqlExpression value)
        {
            Column = column;
            Operator = op;
            Value = value;
        }

        public Condition(string column, string op, SqlExpression value)
        {
            Column = new Identifier(column);
            Operator = op;
            Value = value;
        }

        public IEnumerable<object> GetBindings()
        {
            var bindings = new List<object>();

            if (Column is HasBinding columnHasBinding)
            {
                bindings.AddRange(columnHasBinding.GetBindings());
            }

            if (Value is HasBinding valueHasBinding)
            {
                bindings.AddRange(valueHasBinding.GetBindings());
            }

            return bindings;
        }
    }
}