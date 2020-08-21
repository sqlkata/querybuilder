namespace SqlKata.SqlExpressions
{
    public class Identifier : AbstractSqlExpression
    {
        public string Value { get; }

        public Identifier(string value)
        {
            Value = value;
        }

    }
}