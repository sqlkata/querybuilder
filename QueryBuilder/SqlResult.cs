using System.Text;

namespace SqlKata
{
    public class SqlResult
    {
        public Dictionary<string, object?> NamedBindings = new();
        private readonly List<object?> _bindings;

        public SqlResult()
        {
            _bindings = new List<object?>();
        }

        public SqlResult(IEnumerable<object?> bindings, string? rawSql = null)
        {
            _bindings = bindings.ToList();
            Raw.Append(rawSql);
        }

        public string RawSql => Raw.ToString();
        public StringBuilder Raw { get; set; } = new ();
        public IReadOnlyList<object?> Bindings => _bindings;
        public string Sql { get; set; } = "";
        public void BindingsAdd(object?  value) => _bindings.Add(value);
        public void BindingsAddRange(IEnumerable<object?>  value) => _bindings.AddRange(value);
        public void Prepend(IEnumerable<object?> value) => _bindings.InsertRange(0,value);
        public void ReplaceRaw(string value)
        {
            Raw.Clear();
            Raw.Append(value);
        }
        public override string ToString() => Bindings.BindArgs(RawSql);

        public void PrependOne(object? value)
        {
            _bindings.Insert(0, value);
        }
    }
}
