using System.Collections.Generic;

namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    /// Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class Column : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        /// The column name.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Column
            {
                Engine = Engine,
                Name = Name,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class QueryColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        /// The query for column value calculation.
        /// </value>
        public Query Query { get; set; }
        public override AbstractClause Clone()
        {
            return new QueryColumn
            {
                Engine = Engine,
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class FunctionColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the function name.
        /// </summary>
        /// <value>
        /// The column name.
        /// </value>
        public string Name { get; set; }

        public AbstractColumn On { get; set; }

        public List<AbstractColumn> Parameters { get; set; }

        public List<AbstractClause> Suffixes { get; set; }

        public override AbstractClause Clone()
        {
            var result = new FunctionColumn
            {
                Name = Name,
                Engine = Engine,
                Component = Component,
                On = (AbstractColumn)On?.Clone(),
            };

            if (Parameters != null)
            {
                result.Parameters = new List<AbstractColumn>();
                foreach (var p in Parameters)
                {
                    result.Parameters.Add((AbstractColumn)p.Clone());
                }
            }
            

            if (Suffixes != null)
            {
                result.Suffixes = new List<AbstractClause>();
                foreach (var s in Suffixes)
                {
                    result.Suffixes.Add(s.Clone());
                }
            }

            return result;
        }
    }

    public class RawColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        /// The RAW expression.
        /// </value>
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }
}