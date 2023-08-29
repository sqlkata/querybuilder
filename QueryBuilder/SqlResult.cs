using System.Text;

namespace SqlKata
{
    public class SqlResult
    {
        public Dictionary<string, object?> NamedBindings = new();
        public required Query? Query { get; init; }
        public string RawSql => Raw.ToString();
        public StringBuilder Raw { get; set; } = new ();
        public List<object?> Bindings { get; set; } = new();
        public string Sql { get; set; } = "";

        public void ReplaceRaw(string value)
        {
            Raw.Clear();
            Raw.Append(value);
        }
        public override string ToString() => Bindings.BindArgs(RawSql);
    }
}
