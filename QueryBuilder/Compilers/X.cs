using System.Text;

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

        /// <summary> Wrap a single string in a column identifier. </summary>
        public string WrapName(string value)
        {
            var sb = new StringBuilder();
            WrapName(sb, value);
            return sb.ToString();
        }
        public void WrapName(StringBuilder sb, string value)
        {
            var segments = value.Split(" as ");
            if (segments.Length > 1)
            {
                WrapName(sb, segments[0]);
                sb.Append(" ");
                sb.Append(_columnAsKeyword);
                WrapValue(sb, segments[1]);
            }

            else if (value.Contains("."))
            {
                sb.RenderList(".", value.Split('.'), n => WrapValue(sb, n));
            }
            else
            {
                // If we reach here then the value does not contain an "AS" alias
                // nor dot "." expression, so wrap it as regular value.
                WrapValue(sb, value);
            }
        }

        public (string, string?) SplitAlias(string value)
        {
            var index = value.LastIndexOf(" as ", StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                var before = value[..index];
                var after = value[(index + 4)..];
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
        public void WrapValue(StringBuilder sb, string value)
        {
            if (value == "*")
            {
                sb.Append("*");
                return;
            }
            var val = _capitalize ? value.ToUpperInvariant() : value;
            sb.Append(_openingIdentifier);
            sb.Append(val.Replace(_closingIdentifier, _closingIdentifier + _closingIdentifier));
            sb.Append(_closingIdentifier);
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

        public void AsAlias(StringBuilder sb, string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            sb.Append(_columnAsKeyword);
            WrapValue(sb, input);
        }
    }
}
