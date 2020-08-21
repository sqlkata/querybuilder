using System;

namespace SqlKata.SqlExpressions
{
    public class JsonExtract : AbstractSqlExpression
    {
        public string Column { get; }
        public string Path { get; }

        public JsonExtract(string input)
        {
            if (input.Contains("->"))
            {
                var tokens = input.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                Column = tokens[0];
                Path = tokens[1];
            }
            else
            {
                Column = input;
                Path = "$";
            }
        }

        public JsonExtract(string column, string path)
        {
            Column = column;
            Path = path;
        }

    }
}