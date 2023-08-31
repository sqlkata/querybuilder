using System.Text;
using FluentAssertions;

namespace SqlKata.Compilers
{
    public sealed class Writer
    {
        public IReadOnlyList<object?> Bindings => _bindings;
        public void BindOne(object? value)
        {
            if (value?.AsArray() is { } arr)
                _bindings.AddRange(arr.Cast<object?>());
            else
                _bindings.Add(value);
        }

        public void BindMany(IEnumerable<object?> values)
        {
            foreach (var binding in values)
                BindOne(binding);
        }

        public X X { get; }
        private readonly List<object?> _bindings = new();
        private SqlResult? _ctx;
        private StringBuilder S { get; } = new();
        public static implicit operator string(Writer w) => w.S.ToString();

        public Writer(X x)
        {
            X = x;
        }

        public void List(string separator, int repeatCount, Action<int>? renderItem = null)
        {
            renderItem ??= i => S.Append(i);
            for (var i = 0; i < repeatCount; i++)
            {
                renderItem(i);
                if (i != repeatCount - 1)
                    S.Append(separator);
            }
        }

        public void List<T>(string separator, IEnumerable<T> list, Action<T>? renderItem = null)
        {
            renderItem ??= i => S.Append(i);
            var any = false;
            foreach (var item in list)
            {
                renderItem(item);
                S.Append(separator);
                any = true;
            }

            if (any) S.Length -= separator.Length;
        }

        public void List<T>(string separator, IEnumerable<T?> list, Action<T, int>? renderItem)
            where T : notnull
        {
            renderItem ??= (i, _) => S.Append(i);
            var counter = 0;
            foreach (var item in list)
                if (item != null)
                {
                    renderItem(item, counter);
                    S.Append(separator);
                    counter++;
                }

            if (counter > 0) S.Length -= separator.Length;
        }

        public void WhitespaceSeparated(params Action[] list)
        {
            foreach (var item in list)
            {
                item();
                Whitespace();
            }
            if (S.Length > 0 && S[^1] == ' ') S.Length -= 1;
        }

        public void Assert(string s)
        {
            if (s != S.ToString())
                throw new Exception($"\n\n------Expected------\n{s}\n--------Got---------\n{S}\n\n");
        }

        public void AppendParameter(SqlResult ctx, Query query, object? value)
        {
            Append(Compiler.Parameter(ctx, query, this, value));
        }
        public void Append(string? value) => S.Append(value);
        public void Append(char value) => S.Append(value);
        public void RemoveLast(int howMany) => S.Length -= howMany;

        /// <summary>
        /// Wraps objects like table names, columns, etc.
        /// </summary>
        public void AppendName(string userObjectName)
        {
            X.Wrap(S, userObjectName);
        }

        public void AppendKeyword(string sqlKeyword)
        {
            S.Append(sqlKeyword.ToUpperInvariant());
        }

        public void AppendAsAlias(string? input)
        {
            X.AsAlias(S, input);
        }

        public void AppendRaw(string rawExpression)
        {
            S.Append(X.WrapIdentifiers(rawExpression));
        }
        public void AppendRaw(string rawExpression, IEnumerable<object?> bindings)
        {
            S.Append(X.WrapIdentifiers(rawExpression));
            _bindings.AddRange(bindings);
        }

        public void AppendValue(string value)
        {
            X.WrapValue(S, value);
        }
        public void AppendParameter(object? value)
        {
            S.Append("?");
            BindOne(value);
        }

        public Writer Sub()
        {
            return new Writer(X)
            {
                _ctx = _ctx,
            };
        }

        public void Whitespace()
        {
            if (S.Length > 0 && S[^1] != ' ') S.Append(' ');
        }

        private readonly Stack<SqlResult> _contexts = new();
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<SqlResult> _discards = new();
        public void Push(SqlResult ctx)
        {
            if (_ctx != null)
            {
                _contexts.Push(_ctx);
            }

            _ctx = ctx;
            AssertMatches();
        }
        public void Pop()
        {
            _discards.Add(_ctx = _contexts.Pop());
        }
        public void AssertMatches()
        {
            Bindings.Should().EndWith(_ctx!.Bindings);
        }
        public void AssertMatches(SqlResult ctx)
        {
            if (_ctx != ctx)
                throw new Exception("Wrong context!");
            AssertMatches();
        }

        public void CommaSeparatedParameters(SqlResult ctx, Query query, IEnumerable<object?> values)
        {
            List(", ", values, v => AppendParameter(ctx, query, v));
        }
    }
}
