using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.SqlExpressions
{
    public class Raw : SqlExpression, HasBinding
    {
        public string Value { get; }
        public List<object> Params { get; set; }

        public Raw(string value, params object[] parameters)
        {
            if (value.CountChar('?') != parameters.Count())
            {
                throw new InvalidOperationException(
                    $"Parameters count `{parameters.Count()}` does not match the placeholder '?' count {value.CountChar('?')}"
                );
            };

            Value = value;
        }

        public IEnumerable<object> GetBindings()
        {
            return Params;
        }
    }
}