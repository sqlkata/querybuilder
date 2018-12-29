using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Extensions
{
    public static class Json
    {
        private static List<string> SplitPath(string path)
        {
            var tokens = path.SplitOn("..");
            return tokens.ToList();
        }

        public static Query WhereJson(this Query query, string path, string op, object value)
        {
            return query.ForSqlServer(q => q.WhereRaw(""));
        }
    }
}