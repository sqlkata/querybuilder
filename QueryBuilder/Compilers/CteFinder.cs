using System.Collections.Generic;

namespace SqlKata.Compilers
{
    public class CteFinder
    {
        private readonly string _engineCode;
        private readonly Query _query;
        private HashSet<string> _namesOfPreviousCtes;
        private List<AbstractFrom> _orderedCteList;

        public CteFinder(Query query, string engineCode)
        {
            _query = query;
            _engineCode = engineCode;
        }

        public List<AbstractFrom> Find()
        {
            if (null != _orderedCteList)
                return _orderedCteList;

            _namesOfPreviousCtes = new HashSet<string>();

            _orderedCteList = FindInternal(_query);

            _namesOfPreviousCtes.Clear();
            _namesOfPreviousCtes = null;

            return _orderedCteList;
        }

        private List<AbstractFrom> FindInternal(Query queryToSearch)
        {
            var cteList = queryToSearch.GetComponents<AbstractFrom>("cte", _engineCode);

            var resultList = new List<AbstractFrom>();

            foreach (var cte in cteList)
            {
                if (_namesOfPreviousCtes.Contains(cte.Alias))
                    continue;

                _namesOfPreviousCtes.Add(cte.Alias);
                resultList.Add(cte);

                if (cte is QueryFromClause queryFromClause)
                    resultList.InsertRange(0, FindInternal(queryFromClause.Query));
            }

            return resultList;
        }
    }
}
