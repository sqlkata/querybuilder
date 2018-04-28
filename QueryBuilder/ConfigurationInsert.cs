using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    internal class ConfigurationInsert
    {       
        public string LastInsertCommand { get; set; }
        public string Local { get; set; } = null;
        public bool InsertedCommand = false;
        public string AddSql(string sql)
        {
            if (InsertedCommand)
            {                
                return sql.Insert(sql.IndexOf(Local), LastInsertCommand);                
            }
            return sql + LastInsertCommand;
        }
        public void Replace(IDictionary<string, string> replaces)
        {
            replaces
                .ToList()
                .ForEach(x =>
                {
                    Replace(x.Key, x.Value);
                });
        }

        public void Replace(string oldValue, string newValue)
        {
            LastInsertCommand = LastInsertCommand.Replace(oldValue, newValue);
        }
    }
}
