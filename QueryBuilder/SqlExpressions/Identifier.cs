namespace SqlKata.SqlExpressions
{
    public class Identifier : SqlExpression
    {
        public string Value { get; }

        public Identifier(string value)
        {
            Value = value;
        }

    }
}