namespace SqlKata
{
    public sealed class UnsafeLiteral
    {
        public UnsafeLiteral(string? value, bool replaceQuotes = true)
        {
            value ??= "";

            if (replaceQuotes) value = value.Replace("'", "''");

            Value = value;
        }

        public string Value { get; set; }
    }
}
