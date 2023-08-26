namespace SqlKata
{
    public class UnsafeLiteral
    {
        public UnsafeLiteral(string value, bool replaceQuotes = true)
        {
            if (value == null) value = "";

            if (replaceQuotes) value = value.Replace("'", "''");

            Value = value;
        }

        public string Value { get; set; }
    }
}
