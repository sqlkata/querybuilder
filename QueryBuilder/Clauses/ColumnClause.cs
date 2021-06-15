using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
        public AbstractColumn() { }

        public AbstractColumn(AbstractColumn other)
            : base(other)
        {
            Alias = other.Alias;
        }

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

        public Column() { }

        public Column(Column other)
            : base(other)
        {
            Name = other.Name;
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Column(this);
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

        public QueryColumn() { }

        public QueryColumn(QueryColumn other)
            : base(other)
        {
            Query = other.Query.Clone();
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryColumn(this);
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

    public abstract class AbstractAggregateColumn : AbstractColumn
    {
        public string Column { get; set; } // Aggregate functions accept only a single 'value expression' (for now we implement only column name)
        public enum AggregateDistinct
        {
            aggregateNonDistinct,
            aggregateDistinct,
        };
        public AggregateDistinct Distinct { get; set; }
        public bool IsDistinct { get { return this.Distinct == AggregateDistinct.aggregateDistinct; } }

        public AbstractAggregateColumn() { }

        public AbstractAggregateColumn(AbstractAggregateColumn other)
            : base(other)
        {
            Column = other.Column;
            Distinct = other.Distinct;
        }
    }

    public class AggregateGenericColumn : AbstractAggregateColumn
    {
        public string Type { get; set; } // Min, Max, etc.

        public AggregateGenericColumn() { }

        public AggregateGenericColumn(AggregateGenericColumn other)
            : base(other)
        {
            Type = other.Type;
        }

        public override AbstractClause Clone()
        {
            return new AggregateGenericColumn(this);
        }

        public override string Compile(SqlResult ctx)
        {
            return $"{Type.ToUpperInvariant()}({(IsDistinct ? ctx.Compiler.DistinctKeyword : "")}{new Column { Name = Column }.Compile(ctx)}) {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Alias ?? Type)}";
        }
    }

    public class AggregateAnyValueColumn : AbstractAggregateColumn
    {
        public string Type { get { return "any_value"; } }

        public AggregateAnyValueColumn() { }

        public AggregateAnyValueColumn(AggregateAnyValueColumn other)
            : base(other)
        { }

        public override AbstractClause Clone()
        {
            return new AggregateAnyValueColumn(this);
        }

        public override string Compile(SqlResult ctx)
        {
            // The minimum is a perfectly good "any" value
            return $"{"min".ToUpperInvariant()}({(IsDistinct ? ctx.Compiler.DistinctKeyword : "")}{new Column { Name = Column }.Compile(ctx)}) {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Alias ?? Type)}";
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

        public RawColumn() { }

        public RawColumn(RawColumn other)
            : base(other)
        {
            Expression = other.Expression;
            Bindings = other.Bindings;
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            Debug.Assert(string.IsNullOrEmpty(Alias), "Raw columns cannot have an alias");
            return new RawColumn(this);
        }

        public override string Compile(SqlResult ctx)
        {
            ctx.Bindings.AddRange(Bindings);
            return ctx.Compiler.WrapIdentifiers(Expression);
        }
    }
}
