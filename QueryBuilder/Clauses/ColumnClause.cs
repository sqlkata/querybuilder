using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
        public string Alias { get; set; }

        /// <summary>
        /// This is the first introduced Compile() on columns, only
        /// AggregateColumn implements this at this time.
        /// </summary>
        public abstract string Compile(SqlResult ctx);
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
                Alias = Alias,
            };
        }

        public override string Compile(SqlResult ctx)
        {
            if (!string.IsNullOrWhiteSpace(Alias))
            {
                return $"{ctx.Compiler.Wrap(Name)} {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.Wrap(Alias)}";

            }

            return ctx.Compiler.Wrap(Name);
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
                Alias = Alias,
            };
        }

        public override string Compile(SqlResult ctx)
        {
            var alias = "";

            if (!string.IsNullOrWhiteSpace(Query.QueryAlias))
            {
                alias = $" {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Query.QueryAlias)}";
            }

            var subCtx = ctx.Compiler.CompileSelectQuery(Query);

            ctx.Bindings.AddRange(subCtx.Bindings);

            return "(" + subCtx.RawSql + $"){alias}";
        }
    }

    public class AggregateColumn : AbstractColumn
    {
        public string Type { get; set; } // Min, Max, etc.
        public string Column { get; set; } // Aggregate functions accept only a single 'value expression' (for now we implement only column name)
        public enum AggregateDistinct
        {
            aggregateNonDistinct,
            aggregateDistinct,
        };
        public AggregateDistinct Distinct { get; set; }
        public bool IsDistinct { get { return this.Distinct == AggregateDistinct.aggregateDistinct; } }
        public override AbstractClause Clone()
        {
            return new AggregateColumn
            {
                Engine = Engine,
                Component = Component,
                Alias = Alias,
                Type = Type,
                Column = Column,
                Distinct = Distinct,
            };

        public override string Compile(SqlResult ctx)
        {
            return $"{Type.ToUpperInvariant()}({(IsDistinct ? ctx.Compiler.DistinctKeyword : "")}{new Column { Name = Column }.Compile(ctx)}) {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Alias ?? Type)}";
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
            Debug.Assert(string.IsNullOrEmpty(Alias), "Raw columns cannot have an alias");
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }

        public override string Compile(SqlResult ctx)
        {
            ctx.Bindings.AddRange(Bindings);
            return ctx.Compiler.WrapIdentifiers(Expression);
        }
    }
}
