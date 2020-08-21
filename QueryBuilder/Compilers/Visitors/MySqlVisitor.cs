using System;
using System.Linq;
using SqlKata.SqlExpressions;

namespace SqlKata.Compilers.Visitors
{
    public class MySqlVisitor : AbstractVisitor
    {

        public override string Visit(Concat expression)
        {
            return $"CONCAT({string.Join(", ", expression.Values.Select(x => Visit(x)))})";
        }

        public override string Visit(JsonExtract expression)
        {
            return $"json_extract([{expression.Column}], '{expression.Path}')";
        }

        public override string Visit(Length expression)
        {
            return $"LENGTH({Visit(expression.Value)})";
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