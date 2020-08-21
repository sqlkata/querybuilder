namespace SqlKata.SqlExpressions
{
    public class StringValue : AbstractSqlExpression
    {
        public string Value { get; }

        public StringValue(string value)
        {
            Value = value;
        }

    }
}