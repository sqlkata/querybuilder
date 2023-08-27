namespace SqlKata
{
    public class SqlResult
    {
        public Dictionary<string, object?> NamedBindings = new();
        public required Query? Query { get; init; }
        public string RawSql { get; set; } = "";
        public List<object?> Bindings { get; set; } = new();
        public string Sql { get; set; } = "";

        public override string ToString() => Bindings.BindArgs(RawSql);
    }
}
