using System.Linq.Expressions;
using SqlKata.SqlExpressions;

namespace SqlKata
{

    public class ExpressionClause : AbstractClause
    {
        public SqlExpression Expression { get; set; }
        public override AbstractClause Clone()
        {
            return new ExpressionClause
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
            };
        }
    }

    /// <summary>
    /// Represents an "expression".
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class ColumnExpressionClause : AbstractColumn
    {
        public SqlExpression Expression { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new ColumnExpressionClause
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
            };
        }
    }

    /// <summary>
    /// Represents a select "expression".
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class SelectSqlExpressionClause : ColumnExpressionClause
    {
        public string Alias { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new SelectSqlExpressionClause
            {
                Engine = Engine,
                Alias = Alias,
                Expression = Expression,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents a select "expression".
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class SelectExpressionClause : AbstractColumn
    {
        public Expression Expression { get; set; }
        public string Alias { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new SelectExpressionClause
            {
                Engine = Engine,
                Component = Component,
                Alias = Alias,
                Expression = Expression,
            };
        }
    }

}