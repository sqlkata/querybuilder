using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public override string OpeningIdentifier => "[";
        public override string ClosingIdentifier => "]";
        public override string LastId => "SELECT scope_identity() as Id";

        public SqlServerCompiler()
        {
        }

        public override string EngineCode { get; } = EngineCodes.SqlServer;
        public bool UseLegacyPagination { get; set; } = true;

        public /* friend */ override SqlResult CompileSelectQuery(Query query)
        {
            if (!UseLegacyPagination || !query.HasOffset(EngineCode))
            {
                return base.CompileSelectQuery(query);
            }

            query = query.Clone();

            var ctx = new SqlResult(this)
            {
                Query = query,
            };

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);


            if (!query.HasComponent("select"))
            {
                query.Select("*");
            }

            var order = CompileOrders(ctx) ?? "ORDER BY (SELECT 0)";

            query.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]", ctx.Bindings.ToArray());

            query.ClearComponent("order");


            var result = base.CompileSelectQuery(query);

            if (limit == 0)
            {
                result.RawSql = $"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] >= ?";
                result.Bindings.Add(offset + 1);
            }
            else
            {
                result.RawSql = $"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] BETWEEN ? AND ?";
                result.Bindings.Add(offset + 1);
                result.Bindings.Add(limit + offset);
            }

            return result;
        }

        private class SqlServerAggregatePercentileApproxColumn : SqlKata.AggregatePercentileApproxColumn
        {
            public SqlServerAggregatePercentileApproxColumn() : base() { }

            public SqlServerAggregatePercentileApproxColumn(SqlServerAggregatePercentileApproxColumn other)
                : base(other)
            {
                GroupByClauses = other.GroupByClauses;
            }

            public override AbstractClause Clone()
            {
                return new SqlServerAggregatePercentileApproxColumn(this);
            }

            public override string Compile(SqlResult ctx)
            {
                // percentile_cont(0.9) within group (order by "value") over (partition by ...)
                var column = new Column { Name = Column }.Compile(ctx);
                var partition = String.Join(", ", GroupByClauses.Select(clause => clause.Compile(ctx)));
                partition = partition.Length > 0 ? "PARTITION BY " + partition : "";
                return $"PERCENTILE_CONT({Percentile}) WITHIN GROUP(ORDER BY {column}) OVER({partition}) {ctx.Compiler.ColumnAsKeyword}{ctx.Compiler.WrapValue(Alias ?? Type)}";
            }

            public IEnumerable<Column> GroupByClauses { get; set; } = new List<Column>();
        }

        /**
         * This transforms an APPROX_PERCENTILE column in a SELECT clause to a
         * sub-query with an PERCENTILE_CONT column. Groupings in the original
         * query are repeated in the PARTITION BY of the PERCENTILE_CONT
         * function.
         */
        private void TransformPercentileApprox(Query query)
        {
            // A bit of an optimization for compatibility; we only want to
            // perform the 'real' transform if there is a percentile-approx
            // column in the _current_ query's list of clauses. Otherwise we
            // return the original query as-is, so that there are less total
            // queries that we need to transform and potentially hit an
            // 'unknown' clause type on (c.f. NotImplementedException() below).
            bool hasPercentileApproxColumn = false;
            foreach (var clause in query.Clauses)
            {
                // Expressly only match not-previously-replaced percentiles
                if (clause.GetType() == typeof(SqlKata.AggregatePercentileApproxColumn))
                    hasPercentileApproxColumn = true;

                if (clause is SqlKata.QueryFromClause queryFrom)
                    TransformPercentileApprox(queryFrom.Query);
            }
            if (!hasPercentileApproxColumn)
            {
                return;
            }

            string subqueryAlias = InternalIdentifier(
                query.Clauses
                    .Where(x => x is SqlKata.AggregatePercentileApproxColumn)
                    .Cast<SqlKata.AggregatePercentileApproxColumn>()
                    .First()
                    .Type
            );

            var clauses = query.Clauses
                .OrderBy(clause =>
                {
                    // to ensure selects are added to sub queries before they
                    // are used in group-by or order-by clauses, so that we can
                    // be sure to find them in the alias map.
                    switch (clause.Component)
                    {
                        case "select": return 0;
                        case "group": return 1;
                        case "order": return 2;
                        default: return 999;
                    }
                })
                .ToList();
            query.Clauses = new List<AbstractClause>();
            query.With(subqueryAlias, subQuery =>
            {
                int valueId = 0;
                var aliasMap = new Dictionary<string, string>();
                foreach (var clause in clauses)
                {
                    switch (clause)
                    {
                        case Column column:
                            {
                                switch (column.Component)
                                {
                                    case "select":
                                        {
                                            var clone = column.Clone() as Column;
                                            clone.Alias = $"value_{valueId}";
                                            aliasMap[clone.Name] = clone.Alias;
                                            subQuery.Clauses.Add(clone);
                                            query.SelectAs(($"{subqueryAlias}.value_{valueId}", column.Alias ?? column.Name));
                                            ++valueId;
                                        }; break;
                                    case "group":
                                        {
                                            var clone = clause.Clone() as Column;
                                            clone.Name = $"{subqueryAlias}.{aliasMap[clone.Name]}";
                                            query.Clauses.Add(clone);
                                        }; break;
                                    default: throw new NotImplementedException($"Unhandled column Component: {column.Component}");
                                }
                            }; break;
                        case AbstractAggregateColumn column:
                            {
                                switch (column)
                                {
                                    case AggregatePercentileApproxColumn approxColumn:
                                        {
                                            subQuery.Clauses.Add(
                                                 new SqlServerAggregatePercentileApproxColumn
                                                 {
                                                     Alias = $"value_{valueId}",
                                                     Column = approxColumn.Column,
                                                     Component = approxColumn.Component,
                                                     Distinct = approxColumn.Distinct,
                                                     Engine = approxColumn.Engine,
                                                     Percentile = approxColumn.Percentile,
                                                     GroupByClauses = clauses.Where(x => x is Column group && group.Component == "group").Cast<Column>(),
                                                 }
                                            );
                                            query
                                                .SelectAnyValue($"{subqueryAlias}.value_{valueId}", approxColumn.Alias ?? approxColumn.Type)
                                                .From(subqueryAlias)
                                            ;
                                        }; break;
                                    default:
                                        {
                                            var clone = column.Clone() as AbstractAggregateColumn;
                                            clone.Column = $"{subqueryAlias}.value_{valueId}";
                                            subQuery.SelectAs((column.Column, $"value_{valueId}"));
                                            query.Clauses.Add(clone);
                                        }; break;
                                }
                                ++valueId;
                            }; break;
                        case AbstractFrom from:
                            {
                                subQuery.Clauses.Add(clause.Clone());
                            }; break;
                        case OrderBy orderBy:
                            {
                                var clone = clause.Clone() as OrderBy;
                                clone.Column = $"{subqueryAlias}.{aliasMap[clone.Column]}";
                                query.Clauses.Add(clone);
                            }; break;
                        case AbstractJoin join:
                            {
                                subQuery.Clauses.Add(clause.Clone());
                            }; break;
                        case LimitClause limit:
                            {
                                query.Clauses.Add(clause.Clone());
                            }; break;
                        default: throw new NotImplementedException($"Unhandled clause type: {clause.GetType()}");
                    }
                }
                return subQuery;
            });
        }

        protected override string CompileColumns(SqlResult ctx)
        {
            TransformPercentileApprox(ctx.Query);

            var compiled = base.CompileColumns(ctx);

            if (!UseLegacyPagination)
            {
                return compiled;
            }

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                // top bindings should be inserted first
                ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT") == 0)
                {
                    return "SELECT DISTINCT TOP (?)" + compiled.Substring(15);
                }

                return "SELECT TOP (?)" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(SqlResult ctx)
        {
            if (UseLegacyPagination)
            {
                // in legacy versions of Sql Server, limit is handled by TOP
                // and ROW_NUMBER techniques
                return null;
            }

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            var safeOrder = "";
            if (!ctx.Query.HasComponent("order"))
            {
                safeOrder = "ORDER BY (SELECT 0) ";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"{safeOrder}OFFSET ? ROWS";
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            return $"{safeOrder}OFFSET ? ROWS FETCH NEXT ? ROWS ONLY";
        }

        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }

        public override string CompileTrue()
        {
            return "cast(1 as bit)";
        }

        public override string CompileFalse()
        {
            return "cast(0 as bit)";
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var part = condition.Part.ToUpperInvariant();

            string left;

            if (part == "TIME" || part == "DATE")
            {
                left = $"CAST({column} AS {part.ToUpperInvariant()})";
            }
            else
            {
                left = $"DATEPART({part.ToUpperInvariant()}, {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }

        protected override SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
        {
            var ctx = new SqlResult(this);

            var colNames = string.Join(", ", adHoc.Columns.Select(Wrap));

            var valueRow = string.Join(", ", Enumerable.Repeat("?", adHoc.Columns.Count));
            var valueRows = string.Join(", ", Enumerable.Repeat($"({valueRow})", adHoc.Values.Count / adHoc.Columns.Count));
            var sql = $"SELECT {colNames} FROM (VALUES {valueRows}) AS tbl ({colNames})";

            ctx.RawSql = sql;
            ctx.Bindings = adHoc.Values;

            return ctx;
        }
    }
}
