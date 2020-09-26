using System.Collections.Generic;

namespace SqlKata.SqlExpressions
{
    public class ParamValue : SqlExpression, HasBinding
    {
        public object Value { get; }

        public ParamValue(object value)
        {
            Value = value;
        }

        public IEnumerable<object> GetBindings()
        {
            return new[] { Value };
        }
    }
}