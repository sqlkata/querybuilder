namespace SqlKata
{
    public class UnsafeLiteral
    {
        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value.Replace("'", "''");
            }
        }

        public UnsafeLiteral(string value)
        {
            this.Value = value;
        }

    }
}