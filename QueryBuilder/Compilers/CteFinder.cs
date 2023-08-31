namespace SqlKata.Compilers
{
    public static class CteFinder
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
                    var alias = cte switch
                    {
                        AdHocTableFromClause x => x.Alias,
                        QueryFromClause x => x.Alias!,
                        RawFromClause x => x.Alias!,
                        _ => throw new ArgumentOutOfRangeException(nameof(cte))
                    };
                    if (!already.Add(alias))
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
