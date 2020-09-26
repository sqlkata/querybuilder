using System;
using SqlKata.SqlExpressions;

namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "`";
            LastId = "SELECT last_insert_id() as Id";
        }

        public override string EngineCode { get; } = EngineCodes.MySql;

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);


            if (offset == 0 && limit == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return "LIMIT ?";
            }

            if (limit == 0)
            {

                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.Bindings.Add(offset);
                return "LIMIT 18446744073709551615 OFFSET ?";
            }

            // We have both values

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return "LIMIT ? OFFSET ?";

        }
        public override string Visit(JsonExtract expression)
        {
            return $"json_extract([{expression.Column}], '{expression.Path}')";
        }

        public override string Visit(Cast expression)
        {
            var column = Visit(expression.Value);

            if (expression.TargetType == CastType.Date)
            {
                return $"CAST({column} AS DATE)";
            }

            if (expression.TargetType == CastType.DateTime)
            {
                return $"CAST({column} AS DATETIME)";
            }

            if (expression.TargetType == CastType.Bool)
            {
                return $"CAST({column} AS BINARY)";
            }

            if (expression.TargetType == CastType.Decimal)
            {
                return $"CAST({column} AS DECIMAL)";
            }

            if (expression.TargetType == CastType.Float)
            {
                return $"CAST({column} AS FLOAT)";
            }

            if (expression.TargetType == CastType.Integer)
            {
                return $"CAST({column} AS INT)";
            }

            if (expression.TargetType == CastType.Time)
            {
                return $"CAST({column} AS TIME)";
            }

            if (expression.TargetType == CastType.Varchar)
            {
                return $"CAST({column} AS NVARCHAR)";
            }

            throw new InvalidOperationException($"The provided cast type `{expression.TargetType}` is not available");
        }

    }
}