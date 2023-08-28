namespace SqlKata.Compilers
{
    // TODO: Refactor
    public sealed class WhiteList
    {
        private readonly HashSet<string> _operators = new()
        {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "not like",
            "ilike", "not ilike",
            "like binary", "not like binary",
            "rlike", "not rlike",
            "regexp", "not regexp",
            "similar to", "not similar to"
        };

        private readonly HashSet<string> _userOperators = new();
        public string CheckOperator(string op)
        {
            op = op.ToLowerInvariant();

            var valid = _operators.Contains(op) || _userOperators.Contains(op);

            if (!valid)
                throw new InvalidOperationException(
                    $"The operator '{op}' cannot be used. Please consider white listing it before using it.");

            return op;
        }
    
        public void Whitelist(params string[] operators)
        {
            foreach (var op in operators)
                _userOperators.Add(op);

        }
    }
}
