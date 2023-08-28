namespace SqlKata.Compilers
{
    public sealed class X
    {
        private readonly bool _capitalize;
        private readonly string _openingIdentifier;
        private readonly string _closingIdentifier;
        private readonly string _columnAsKeyword;
        private const string EscapeCharacter = "\\";

        public X(string openingIdentifier,
            string closingIdentifier,
            string columnAsKeyword,
            bool capitalize = false)
        {
            _capitalize = capitalize;
            _openingIdentifier = openingIdentifier;
            _closingIdentifier = closingIdentifier;
            _columnAsKeyword = columnAsKeyword;
        }

        /// <summary>
        ///     Wrap a single string in a column identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Wrap(string value)
        {
            var segments = value.Split(" as ");
            if (segments.Length > 1)
                return $"{Wrap(segments[0])} {_columnAsKeyword}{WrapValue(segments[1])}";

            if (value.Contains("."))
                return string.Join(".", value.Split('.').Select((x, _) => WrapValue(x)));

            // If we reach here then the value does not contain an "AS" alias
            // nor dot "." expression, so wrap it as regular value.
            return WrapValue(value);
        }

        public (string, string?) SplitAlias(string value)
        {
            var index = value.LastIndexOf(" as ", StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                var before = value.Substring(0, index);
                var after = value.Substring(index + 4);
                return (before, $" {_columnAsKeyword}{after}");
            }

            return (value, null);
        }

        /// <summary>
        ///     Wrap a single string in keyword identifiers.
        /// </summary>
        public string WrapValue(string value)
        {
            var result = value.Brace(_openingIdentifier, _closingIdentifier);
            return _capitalize ? result.ToUpperInvariant() : result;
        }
        public string WrapIdentifiers(string input)
        {
            return input

                // deprecated
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "{", _openingIdentifier)
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "}", _closingIdentifier)
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "[", _openingIdentifier)
                .ReplaceIdentifierUnlessEscaped(EscapeCharacter, "]", _closingIdentifier);
        }
        public string AsAlias(string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? ""
                : $" {_columnAsKeyword}{WrapValue(input)}";
        }
    }
}
