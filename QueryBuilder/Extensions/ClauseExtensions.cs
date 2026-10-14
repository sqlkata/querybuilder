using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public static class ClauseExtensions
    {
        public static IEnumerable<T> ForEngine<T>(this IEnumerable<T> clauses, string engineCode) where T : AbstractClause
        {
            if (clauses == null)
            {
                return Enumerable.Empty<T>();
            }

            return clauses.Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine);
        }

        public static T ForEngineOrDefault<T>(this IEnumerable<T> clauses, string engineCode) where T : AbstractClause
        {
            if (clauses == null)
            {
                return null;
            }

            var list = clauses as IList<T> ?? clauses.ToList();
            return list.FirstOrDefault(c => c.Engine == engineCode) ?? list.FirstOrDefault(c => c.Engine == null);
        }

        public static void ClearForEngine<T>(this List<T> clauses, string engineCode = null) where T : AbstractClause
        {
            if (clauses == null) return;
            if (engineCode == null)
            {
                clauses.Clear();
            }
            else
            {
                clauses.RemoveAll(x => x.Engine == null || x.Engine == engineCode);
            }
        }
    }
}
