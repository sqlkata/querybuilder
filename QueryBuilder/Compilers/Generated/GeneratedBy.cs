using SqlKata;

namespace QueryBuilder.Compilers.Generated
{
    public abstract class GeneratedBy : IGeneratedBy
    {
        public string CommandSqlLastId { get; set; }

        public GeneratedByType GeneratedByType { get; private set; }

        public string FindWord { get; private set; }

        public string ColumnName { get; private set; }

        public GeneratedBy(string commandSqlLastId, GeneratedByType generatedByType, string findWord = "", string columnName = "")
        {
            CommandSqlLastId = commandSqlLastId;
            GeneratedByType = generatedByType;
            FindWord = findWord;
            ColumnName = columnName;
        }

        public void Render(SqlResult result)
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
                        result.RawSql.Insert(result.RawSql.IndexOf(FindWord), CommandSqlLastId);
                        break;
                    }            
            }
        }
    }
}
