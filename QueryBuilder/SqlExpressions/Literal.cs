namespace SqlKata.SqlExpressions
{
    public class Literal : AbstractSqlExpression
    {
        public string Value { get; }

        public Literal(string value)
        {
            Value = value;
        }

        public Literal(object value)
        {
            Value = value.ToString();
        }

    }
}