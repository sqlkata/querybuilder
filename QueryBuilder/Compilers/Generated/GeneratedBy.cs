using SqlKata;

namespace QueryBuilder.Compilers.Generated
{
    public abstract class GeneratedBy : IGeneratedBy
    {
        public string CommandSqlLastId { get; set; }

        public GeneratedByType GeneratedByType { get; private set; }

        public string Find { get; private set; }

        public string Column { get; private set; }

        public GeneratedBy(string commandSqlLastId, GeneratedByType generatedByType, string find = "", string column = "")
        {
            CommandSqlLastId = commandSqlLastId;
            GeneratedByType = generatedByType;
            Find = find;
            Column = column;
        }

        public void Merge(SqlResult result)
        {            
            switch(GeneratedByType)
            {                
                case GeneratedByType.Last:
                    {
                        result.RawSql += ";" + CommandSqlLastId;
                        break;
                    }
                case GeneratedByType.Insert:
                    {
                        result.RawSql =
                            result.RawSql.Insert(result.RawSql.IndexOf(Find), CommandSqlLastId + " ");
                        break;
                    }            
            }
        }
    }
}
