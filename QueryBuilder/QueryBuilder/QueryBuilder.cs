using System.Collections.Immutable;

namespace SqlKata
{
    public static class QueryBuilder
    {
        public static Q Build(Query query)
        {
            return query.Method switch
            {
                "insert" => CompileInsertQuery(query),
                "select" => CompileSelectQuery(query),
                _ => throw new NotImplementedException()
            };
        }

        private static Q CompileSelectQuery(Query query)
        {
            return new QList(" ",
                CompileColumns(query),
                CompileFrom(query),
                CompileWheres(query));
        }

        private static Q CompileInsertQuery(Query query)
        {
            var from = query.Components.GetOneComponent<AbstractFrom>("from");
            if (from is null)
                throw new InvalidOperationException("No table set to insert");

            if (from is not FromClause and not RawFromClause)
                throw new InvalidOperationException("Invalid table expression");


            var inserts = query.Components.GetComponents<AbstractInsertClause>("insert");
            if (inserts[0] is InsertQueryClause iqc)
                return new QInsertQuery(iqc);

            var first = (InsertClause)inserts[0];
            var columns = new QInsertColumns(first.Columns);
            var values = inserts
                .Cast<InsertClause>()
                .Select(c => new QInsertValues(
                    c.Values.Select(Parametrize).ToImmutableArray()))
                .ToArray();
            var returnId = first.ReturnId;
            return new QValueInsert(from, columns, values, returnId);
        }

        private static Q CompileColumns(Query query)
        {
            var aggregate = query.GetOneComponent<AggregateClause>("aggregate");
            if (aggregate != null)
            {
                if (aggregate.Columns.Length != 1)
                    return new QLiteral("SELECT 1");
                return new QList(" ",
                    new QLiteral("SELECT"),
                    new QPrefix(aggregate.Type.ToUpperInvariant(),
                        new QRoundBraces(
                            new QCondHeader(query.IsDistinct, "DISTINCT",
                                new QColumn(aggregate.Columns[0])))),
                    new QAsAlias(aggregate.Type));
            }

            var columns = query.Components
                .GetComponents<AbstractColumn>("select")
                .Select(CompileColumn)
                .ToImmutableArray();

            return new QSelect(query.IsDistinct, columns);
        }

        private static QColumn CompileColumn(AbstractColumn column)
        {
            //if (column is RawColumn raw)
            //{
            //    ctx.Bindings.AddRange(raw.Bindings);
            //    return XService.WrapIdentifiers(raw.Expression);
            //}
            //
            //if (column is QueryColumn queryColumn)
            //{
            //    var alias = XService.AsAlias(queryColumn.Query.QueryAlias);
            //    var subCtx = CompileSelectQuery(queryColumn.Query);
            //
            //    ctx.Bindings.AddRange(subCtx.Bindings);
            //
            //    return "(" + subCtx.RawSql + $"){alias}";
            //}
            //
            //if (column is AggregatedColumn aggregatedColumn)
            //{
            //    var agg = aggregatedColumn.Aggregate.ToUpperInvariant();
            //
            //    var (col, alias) = XService.SplitAlias(CompileColumn(ctx, aggregatedColumn.Column));
            //
            //    var filterCondition = CompileFilterConditions(ctx, aggregatedColumn);
            //
            //    if (string.IsNullOrEmpty(filterCondition)) return $"{agg}({col}){alias}";
            //
            //    if (SupportsFilterClause) return $"{agg}({col}) FILTER (WHERE {filterCondition}){alias}";
            //
            //    return $"{agg}(CASE WHEN {filterCondition} THEN {col} END){alias}";
            //}

            return new QColumn(((Column)column).Name);
        }

        private static QFrom? CompileFrom(Query query)
        {
            var from = query.Components.GetOneComponent<AbstractFrom>("from");
            return from != null
                ? new QFrom(CompileTableExpression(from))
                : null;

            QTableExpression CompileTableExpression(AbstractFrom arg)
            {
                // if (from is RawFromClause raw)
                // {
                //     ctx.Bindings.AddRange(raw.Bindings);
                //     return XService.WrapIdentifiers(raw.Expression);
                // }
                //
                // if (from is QueryFromClause queryFromClause)
                // {
                //     var fromQuery = queryFromClause.Query;
                //
                //     var alias = string.IsNullOrEmpty(fromQuery.QueryAlias)
                //         ? ""
                //         : $" {TableAsKeyword}" + XService.WrapValue(fromQuery.QueryAlias);
                //
                //     var subCtx = CompileSelectQuery(fromQuery);
                //
                //     ctx.Bindings.AddRange(subCtx.Bindings);
                //
                //     return "(" + subCtx.RawSql + ")" + alias;
                // }

                if (arg is FromClause fromClause)
                    return new QFromClause(fromClause);

                throw InvalidClauseException("TableExpression", arg);
            }
        }

        private static QWhere? CompileWheres(Query query)
        {
            var conditions = query.Components.GetComponents<AbstractCondition>("where");
            if (conditions.Count == 0) return null;
            return new QWhere(conditions
                .Select((t, i) => CompileCondition(t, i == 0))
                .ToImmutableArray());

            QConditionTag CompileCondition(AbstractCondition clause, bool isFirst)
            {
                return new QConditionTag(
                    isFirst ? null : clause.IsOr,
                    ChooseCondition(clause));
            }

            Q ChooseCondition(AbstractCondition clause)
            {
                return clause switch
                {
                    BasicCondition c => BasicCondition(clause.IsNot, c),
                    NullCondition n => NullCondition(clause.IsNot, n),
                    BooleanCondition b => BooleanCondition(clause.IsNot, b),
                    SubQueryCondition sub => Condition(sub),
                    TwoColumnsCondition cc =>TwoColumnsCondition(clause.IsNot,cc),
                          
                    _ => throw new ArgumentOutOfRangeException(clause.GetType().Name)
                };
            }

            Q BasicCondition(bool isNot, BasicCondition c) =>
                new QNot(isNot, new QList(" ",
                    new QColumn(c.Column),
                    new QOperator(c.Operator),
                    Parametrize(c.Value)));

            Q NullCondition(bool isNot, NullCondition n) =>
                new QList(" ",
                    new QColumn(n.Column),
                    new QNullCondition(isNot));

            Q TwoColumnsCondition(bool isNot, TwoColumnsCondition cc) =>
                new QCondHeader(isNot, "NOT",
                    new QList(" ",
                        new QColumn(cc.First),
                        new QOperator(cc.Operator),
                        new QColumn(cc.Second)));

            Q BooleanCondition(bool isNot, BooleanCondition b) =>
                new QList(" ",
                    new QColumn(b.Column),
                    new QOperator(isNot ? "!=" : "="),
                    new QBoolean(b.Value));

            Q Condition(SubQueryCondition sub) =>
                new QList(" ",
                    new QRoundBraces(CompileSelectQuery(sub.Query)),
                    new QOperator(sub.Operator),
                    Parametrize(sub.Value));
        }

        private static InvalidCastException InvalidClauseException(string section, AbstractClause clause)
        {
            return new InvalidCastException(
                $"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");
        }

        private static QParameter Parametrize(object? parameter)
        {
            return parameter switch
            {
                UnsafeLiteral literal => new QUnsafeLiteral(literal),
                Variable variable => new QVariable(variable),
                _ => new QObject(parameter)
            };
        }
    }
}
