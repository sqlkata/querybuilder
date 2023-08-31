namespace SqlKata
{
    public class SqlResult
    {
        public Dictionary<string, object?> NamedBindings = new();
        private List<object?> _bindings = new();

        public string? RawSql { get; private set; }
        public IReadOnlyList<object?> Bindings => _bindings;
        public string Sql { get; set; } = "";

        public void ReplaceRaw(string value) => RawSql = value;
        public override string? ToString() =>
            RawSql == null ? null : Bindings.BindArgs(RawSql);

        public void ReplaceBindings(IReadOnlyList<object?> writerBindings)
        {
            _bindings = writerBindings.ToList();
        }
    }
}
