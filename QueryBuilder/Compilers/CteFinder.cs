using System.Diagnostics;

namespace SqlKata.Compilers
{
    public sealed class CteFinder
    {
        public static List<AbstractFrom> Find(Query queryToSearch, string? engineCode)
        {
            var already = new HashSet<string>();
            return FindRecursively(queryToSearch);

            List<AbstractFrom> FindRecursively(Query query)
            {
                var result = new List<AbstractFrom>();
                foreach (var cte in query.GetComponents<AbstractFrom>("cte", engineCode))
                {
                    Debug.Assert(cte.Alias != null, "All CTE components have alias!");
                    if (!already.Add(cte.Alias))
                        continue;
                    if (cte is QueryFromClause qfc)
                        result.InsertRange(0, FindRecursively(qfc.Query));
                    result.Add(cte);
                }

                return result;
            }
        }
    }
}
