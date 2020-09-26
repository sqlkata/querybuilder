using System.Collections.Generic;

namespace SqlKata.SqlExpressions
{
    public class SelectAlias : SqlExpression, HasBinding
    {
        private readonly string _alias;

        public string Alias { get { return _alias; } }

        public SqlExpression Value { get; }

        public SelectAlias(SqlExpression value, string alias)
        {
            Value = value;
            this._alias = alias;
        }

        public IEnumerable<object> GetBindings()
        {
            return Value is HasBinding hasBinding ? hasBinding.GetBindings() : null;
        }
    }
}