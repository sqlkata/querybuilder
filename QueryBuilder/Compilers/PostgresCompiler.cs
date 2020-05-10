using System.Collections.Generic;

namespace SqlKata.Compilers
{
    public class PostgresCompiler : Compiler
    {
        public PostgresCompiler()
        {
            LastId = "SELECT lastval() AS id";
        }

        public override string EngineCode { get; } = EngineCodes.PostgreSql;

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"{column}::time";
            }
            else if (condition.Part == "date")
            {
                left = $"{column}::date";
            }
            else
            {
                left = $"DATE_PART('{condition.Part.ToUpperInvariant()}', {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected override string CompileInsertQueryString(string table, List<string> columns, string rawValues, List<string> returnColumns)
        {
            var rawSql = base.CompileInsertQueryString(table, columns, rawValues, returnColumns);

            if (returnColumns.Count > 0)
            {
                var returning = string.Join(", ", WrapArray(returnColumns));
                rawSql += $" RETURNING {returning}";
            }

            return rawSql;
        }

        protected override string CompileUpdateQueryString(string table, string sets, string where, List<string> returnColumns)
        {
            var rawSql = $"UPDATE {table} SET {sets}{where}";

            if (returnColumns.Count > 0)
            {
                var returning = string.Join(", ", WrapArray(returnColumns));
                rawSql += $" RETURNING {returning}";
            }

            return rawSql;
        }
    }
}
