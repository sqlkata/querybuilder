using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlKata.SqlExpressions;

namespace SqlKata.Compilers.Visitors
{
    public abstract class AbstractVisitor : SqlExpressionVisitorInterface
    {
        // public string Visit(AbstractSqlExpression expression)
        // {
        //     return Visit((dynamic)expression);
        // }

        public abstract string Visit(JsonExtract expression);
        public abstract string Visit(Cast expression);

        public string Visit(StringValue expression)
        {
            return "'" + expression.Value.Replace("'", "\'") + "'";
        }

        public string Visit(Literal expression)
        {
            return expression.Value;
        }

        public virtual string Visit(Function expression)
        {
            return $"{expression.Name}({string.Join(", ", expression.Values.Select(x => Visit(x)))})";
        }

        public string Visit(Identifier expression)
        {
            return $"[{expression.Value}]";
        }

        public string Visit(Expression expression)
        {
            return Visit((dynamic)expression);
        }

        public string Visit(ConstantExpression expression)
        {
            if (expression.Value.GetType() == typeof(string))
            {
                return "'" + expression.Value.ToString() + "'";
            }

            return expression.Value.ToString();
        }

        public string Visit(BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);

            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    return $"{left} + {right}";
                case ExpressionType.Subtract:
                    return $"{left} - {right}";
                case ExpressionType.Coalesce:
                    return $"COALESCE({left}, {right})";
                case ExpressionType.Divide:
                    return $"{left} / {right}";
                case ExpressionType.Multiply:
                    return $"{left} * {right}";
                case ExpressionType.AndAlso:
                    return $"({left} AND {right})";
                case ExpressionType.OrElse:
                    return $"({left} OR {right})";
                case ExpressionType.GreaterThan:
                    return $"{left} > {right}";
                case ExpressionType.GreaterThanOrEqual:
                    return $"{left} >= {right}";
                case ExpressionType.LessThan:
                    return $"{left} < {right}";
                case ExpressionType.LessThanOrEqual:
                    return $"{left} <= {right}";
                case ExpressionType.Equal:
                    return $"{left} = {right}";
                case ExpressionType.NotEqual:
                    return $"{left} != {right}";
                case ExpressionType.Modulo:
                    return $"{left} % {right}";
                case ExpressionType.Power:
                    return $"POWER({left}, {right})";
            }

            return $"/* Unkown binary expression: {expression.ToString()} */";
        }

        public string Visit(Expression<Func<bool>> expression)
        {
            return Visit(expression.Body);
        }

        public string Visit(UnaryExpression expression)
        {
            var operand = Visit(expression.Operand);

            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return $"NOT ({operand})";

                case ExpressionType.Negate:
                    return $"-{operand}";
            }

            return $"/* Unkown unary expression: {expression.ToString()} */";
        }

        public string Visit(ParameterExpression expression)
        {
            return Visit(new Identifier(expression.Name));
        }

        public string Visit(ConditionalExpression expression)
        {
            return $"(CASE WHEN {Visit(expression.Test)} THEN {Visit(expression.IfTrue)} ELSE {Visit(expression.IfFalse)} END)";
        }

        public string Visit(Wrap expression)
        {
            return $"({Visit(expression.Body)})";
        }

        public string Visit(Case expression)
        {
            if (!expression.Cases.Any()) return "/* empty case */";

            var result = new List<string> { };

            if (expression.Test != null)
            {
                result.Add(Visit(expression.Test));
            }

            foreach (var item in expression.Cases)
            {
                result.Add($"WHEN {Visit(item.Key)} THEN {Visit(item.Value)}");
            }

            if (expression.ElseDefault != null)
            {
                result.Add($"ELSE {Visit(expression.ElseDefault)}");
            }

            return $"CASE {string.Join(" ", result)} END";

        }

        public string Visit(BlockExpression expression)
        {
            return string.Join(" ", expression.Expressions.Select(x => Visit(x)));
        }
    }
}