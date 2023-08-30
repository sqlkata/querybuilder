using System.Text;
using FluentAssertions;

namespace SqlKata.Compilers
{
    public sealed class Writer
    {
        public IReadOnlyList<object?> Bindings => _bindings;
        public void BindOne(object? value)
        {
            _bindings.Add(value);
        }

        public void BindMany(IEnumerable<object?> values)
        {
            foreach (var binding in values) BindOne(binding);
        }

        private readonly X _x;
        private readonly List<object?> _bindings = new();
        private SqlResult? _ctx;
        public StringBuilder S { get; } = new();
        public static implicit operator string(Writer w) => w.S.ToString();

        public Writer(X x)
        {
            _x = x;
        }

        public void List<T>(string separator, IEnumerable<T?> list, Action<T>? renderItem = null)
            where T: notnull
        {
            renderItem ??= i => S.Append(i);
            var any = false;
            foreach (var item in list)
                if (item != null)
                {
                    renderItem(item);
                    S.Append(separator);
                    any = true;
                }

            if (any) S.Length -= separator.Length;
        }

        public void List<T>(string separator, IEnumerable<T?> list, Action<T, int>? renderItem)
            where T: notnull
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

        public void AppendName(string userObjectName)
        {
            _x.Wrap(S, userObjectName);
        }

        public void AppendKeyword(string sqlKeyword)
        {
            S.Append(sqlKeyword.ToUpperInvariant());
        }

        public void AppendAsAlias(string? input)
        {
            _x.AsAlias(S, input);
        }

        public void AppendRaw(string rawExpression)
        {
            S.Append(_x.WrapIdentifiers(rawExpression));
        }

        public Writer Sub()
        {
            return new Writer(_x)
            {
                _ctx = _ctx
            };
        }

        public void Whitespace()
        {
            if (S.Length > 0 && S[^1] != ' ') S.Append(' ');
        }

        public void SetCtx(SqlResult ctx)
        {
            _ctx = ctx;
            AssertMatches();
        }
        public void AssertMatches()
        {
            Bindings.Should().EndWith(_ctx!.Bindings);
        }
    }
}
