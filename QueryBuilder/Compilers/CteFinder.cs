namespace SqlKata.Compilers
{
    public sealed class CteFinder
    {
        private readonly string _engineCode;
        private readonly Query _query;
        private List<AbstractFrom>? _orderedCteList;

        public CteFinder(Query query, string engineCode)
        {
            _query = query;
            _engineCode = engineCode;
        }

        public List<AbstractFrom> Find()
        {
            if (_orderedCteList != null)
                return _orderedCteList;

            var namesOfPreviousCtes = new HashSet<string>();
            List<AbstractFrom> FindInternal(Query queryToSearch)
            {
                var cteList = queryToSearch.GetComponents<AbstractFrom>("cte", _engineCode);

                var resultList = new List<AbstractFrom>();

                foreach (var cte in cteList)
                {
                    if (namesOfPreviousCtes.Contains(cte.Alias))
                        continue;

                    namesOfPreviousCtes.Add(cte.Alias);
                    resultList.Add(cte);

                    if (cte is QueryFromClause queryFromClause)
                        resultList.InsertRange(0, FindInternal(queryFromClause.Query));
                }

                return resultList;
            }
            return _orderedCteList = FindInternal(_query);
        }
    }
}
