using System.Collections.Generic;

namespace SqlKata.Compilers
{
    public class CteFinder
    {
        private readonly Query query;
        private readonly string engineCode;
        private HashSet<string> namesOfPreviousCtes;
        private List<AbstractFrom> orderedCteList;
        
        public CteFinder(Query query, string engineCode)
        {
            this.query = query;
            this.engineCode = engineCode;
        }

        public List<AbstractFrom> Find()
        {
            if (null != orderedCteList)
                return orderedCteList;

            namesOfPreviousCtes = new HashSet<string>();
            
            orderedCteList = FindInternal(query);
            
            namesOfPreviousCtes.Clear();
            namesOfPreviousCtes = null;

            return orderedCteList;
        }

        private List<AbstractFrom> FindInternal(Query queryToSearch)
        {
            var cteList = queryToSearch.GetComponents<AbstractFrom>("cte", engineCode);

            var resultList = new List<AbstractFrom>();
            
            foreach (var cte in cteList)
            {
                if (namesOfPreviousCtes.Contains(cte.Alias))
                    continue;

                namesOfPreviousCtes.Add(cte.Alias);
                resultList.Add(cte);

                if (cte is QueryFromClause queryFromClause)
                {
                    resultList.InsertRange(0, FindInternal(queryFromClause.Query));
                }
            }

            return resultList;
        }
    }
}
