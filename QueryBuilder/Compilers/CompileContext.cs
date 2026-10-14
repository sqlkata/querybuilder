using System;
using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers
{
    public class CompileContext
    {
        public StringBuilder Buffer { get; } = new();
        public List<object> Bindings { get; set; } = [];
        public string RawSql { get; set; } = "";
        public string ParameterPlaceholder { get; set; } = "?";
        public string EscapeCharacter { get; set; } = "\\";

        private readonly Stack<Query> _queryStack = new();

        public Query CurrentQuery => _queryStack.Count > 0 ? _queryStack.Peek() : null;

        public Query Query
        {
            get => CurrentQuery;
            set
            {
                if (_queryStack.Count > 0)
                {
                    _queryStack.Pop();
                }

                if (value != null)
                {
                    _queryStack.Push(value);
                }
            }
        }

        public IReadOnlyCollection<Query> QueryStack => _queryStack;

        public CompileContext() : this(null, "?", "\\")
        {
        }

        public CompileContext(Query query, string parameterPlaceholder = "?", string escapeCharacter = "\\")
        {
            ParameterPlaceholder = parameterPlaceholder;
            EscapeCharacter = escapeCharacter;
            if (query != null)
            {
                _queryStack.Push(query);
            }
        }

        public void PushQuery(Query query)
        {
            if (query != null)
            {
                _queryStack.Push(query);
            }
        }

        public Query PopQuery()
        {
            return _queryStack.Count > 0 ? _queryStack.Pop() : null;
        }

        public object FindVariable(string name)
        {
            foreach (var q in _queryStack)
            {
                if (q?.Variables != null && q.Variables.TryGetValue(name, out var value))
                {
                    return value;
                }
            }

            throw new Exception($"Variable '{name}' not found");
        }

        public CompileContext Write(string value)
        {
            Buffer.Append(value);
            return this;
        }

        public CompileContext Write(char value)
        {
            Buffer.Append(value);
            return this;
        }

        public CompileContext EnsureSpace()
        {
            if (Buffer.Length > 0)
            {
                var lastChar = Buffer[Buffer.Length - 1];
                if (lastChar != ' ' && lastChar != '\n')
                {
                    Buffer.Append(' ');
                }
            }
            return this;
        }

        public override string ToString() => Buffer.ToString();
    }
}
