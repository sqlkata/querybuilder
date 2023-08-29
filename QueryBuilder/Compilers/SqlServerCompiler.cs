namespace SqlKata.Compilers
{
    public class SqlServerCompiler : Compiler
    {
        public SqlServerCompiler()
        {
            XService = new X("[", "]", "AS ");
            LastId = "SELECT scope_identity() as Id";
            EngineCode = EngineCodes.SqlServer;
        }

        public bool UseLegacyPagination { get; set; } = false;

        public override SqlResult CompileSelectQuery(Query query)
        {
            if (!UseLegacyPagination || !query.HasOffset(EngineCode))
                return base.CompileSelectQuery(query);

            query = query.Clone();

            var ctx = new SqlResult
            {
                Query = query
            };

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);


            if (!query.HasComponent("select")) query.Select("*");

            var order = CompileOrders(ctx, new Writer(XService)) ?? "ORDER BY (SELECT 0)";

            query.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]",
                ctx.Bindings.ToArray());

            query.RemoveComponent("order");


            var result = base.CompileSelectQuery(query);

            if (limit == 0)
            {
                result.ReplaceRaw($"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] >= ?");
                result.Bindings.Add(offset + 1);
            }
            else
            {
                result.ReplaceRaw($"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] BETWEEN ? AND ?");
                result.Bindings.Add(offset + 1);
                result.Bindings.Add(limit + offset);
            }

            return result;
        }

        protected override string CompileColumns(SqlResult ctx, Writer writer)
        {
            var compiled = base.CompileColumns(ctx, writer.Sub());

            if (!UseLegacyPagination)
            {
                writer.S.Append(compiled);
                return writer;
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

                ctx.Query.RemoveComponent("limit");

                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT", StringComparison.Ordinal) == 0)
                {
                    writer.S.Append("SELECT DISTINCT TOP (?)");
                    writer.S.Append(compiled.Substring(15));
                    return writer;
                }

                writer.S.Append("SELECT TOP (?)");
                writer.S.Append(compiled.Substring(6));
                return writer;
            }

            writer.S.Append(compiled);
            return writer;
        }

        public override string? CompileLimit(SqlResult ctx, Writer writer)
        {
            if (UseLegacyPagination)
                // in legacy versions of Sql Server, limit is handled by TOP
                // and ROW_NUMBER techniques
                return null;

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0) return null;

            if (!ctx.Query.HasComponent("order")) writer.S.Append("ORDER BY (SELECT 0) ");

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                writer.S.Append("OFFSET ? ROWS");
                return writer;
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);
            writer.S.Append("OFFSET ? ROWS FETCH NEXT ? ROWS ONLY");
            return writer;
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
            var column = XService.Wrap(condition.Column);
            var part = condition.Part.ToUpperInvariant();

            string left;

            if (part == "TIME" || part == "DATE")
                left = $"CAST({column} AS {part.ToUpperInvariant()})";
            else
                left = $"DATEPART({part.ToUpperInvariant()}, {column})";

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot) return $"NOT ({sql})";

            return sql;
        }

        protected override SqlResult CompileAdHocQuery(AdHocTableFromClause adHoc)
        {
            var ctx = new SqlResult(){Query = null};

            var colNames = string.Join(", ", adHoc.Columns.Select(value => XService.Wrap(value)));

            var valueRow = string.Join(", ", Enumerable.Repeat("?", adHoc.Columns.Length));
            var valueRows = string.Join(", ",
                Enumerable.Repeat($"({valueRow})", adHoc.Values.Length / adHoc.Columns.Length));
            var sql = $"SELECT {colNames} FROM (VALUES {valueRows}) AS tbl ({colNames})";

            ctx.Raw.Append(sql);
            ctx.Bindings = adHoc.Values.ToList();

            return ctx;
        }
    }
}
