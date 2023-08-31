using System.Text;

namespace SqlKata
{
    public class SqlResult
    {
        public Dictionary<string, object?> NamedBindings = new();
        private readonly List<object?> _bindings = new();

        public string RawSql => Raw.ToString();
        private StringBuilder Raw { get; } = new ();
        public IReadOnlyList<object?> Bindings => _bindings;
        public string Sql { get; set; } = "";
        public void BindingsAdd(object?  value) => _bindings.Add(value);
        public void BindingsAddRange(IEnumerable<object?>  value) => _bindings.AddRange(value);

        public void ReplaceRaw(string value)
        {
            Raw.Clear();
            Raw.Append(value);
        }
        public override string ToString() => Bindings.BindArgs(RawSql);
    }
}
