namespace SqlKata.Compilers
{
    public static class CteFinder
    {
        public static List<AbstractFrom> FindCte(this Query queryToSearch, string? engineCode)
        {
            var already = new HashSet<string>();
            return FindRecursively(queryToSearch);

            List<AbstractFrom> FindRecursively(Query query)
            {
                var result = new List<AbstractFrom>();
                foreach (var cte in query.GetComponents<AbstractFrom>("cte", engineCode))
                {
                    if (!already.Add(cte.GetAlias()))
                        continue;
                    if (cte is QueryFromClause qfc)
                        result.InsertRange(0, FindRecursively(qfc.Query));
                    result.Add(cte);
                }

                return result;
            }
        }

        private static string GetAlias(this AbstractFrom cte) =>
            cte switch
            {
                AdHocTableFromClause x => x.Alias,
                QueryFromClause x => x.Alias!,
                RawFromClause x => x.Alias!,
                _ => throw new ArgumentOutOfRangeException(nameof(cte))
            };
    }
}
