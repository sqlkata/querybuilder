using System.Text;

namespace SqlKata.Compilers
{
    public sealed class Writer
    {
        private readonly X _x;
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

            if (any) S.Remove(S.Length - separator.Length, separator.Length);
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

        public void AppendAsAlias(string aggregateType)
        {
            _x.AsAlias(S, aggregateType);
        }

        public void AppendRaw(string rawExpression)
        {
            S.Append(_x.WrapIdentifiers(rawExpression));
        }

        public Writer Sub()
        {
            return new Writer(_x);
        }
    }
}
