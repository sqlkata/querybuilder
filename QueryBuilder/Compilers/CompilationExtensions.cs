namespace SqlKata.Compilers
{
    public static class CompilationExtensions
    {
        public static SqlResult Compile(this Compiler compiler, Query query)
        {
            var ctx = compiler.CompileRaw(query);
            ctx = compiler.PrepareResult(ctx);
            return ctx;
        }

        public static SqlResult Compile(this Compiler compiler, IEnumerable<Query> queries)
        {
            var compiled = queries.Select(compiler.CompileRaw).ToArray();
            var bindings = compiled.Select(r => r.Bindings).ToArray();
            var totalBindingsCount = bindings.Select(b => b.Count).Aggregate((a, b) => a + b);

            var combinedBindings = new List<object?>(totalBindingsCount);
            foreach (var cb in bindings) combinedBindings.AddRange(cb);

            var ctx = new SqlResult
            {
                Query = null,
                RawSql = compiled.Select(r => r.RawSql).Aggregate((a, b) => a + ";\n" + b),
                Bindings = combinedBindings
            };

            ctx = compiler.PrepareResult(ctx);

            return ctx;
        }

        private static SqlResult CompileRaw(this Compiler compiler, Query query)
        {
            SqlResult ctx;

            if (query.Method == "insert")
            {
                ctx = compiler.CompileInsertQuery(query);
            }
            else if (query.Method == "update")
            {
                ctx = compiler.CompileUpdateQuery(query);
            }
            else if (query.Method == "delete")
            {
                ctx = compiler.CompileDeleteQuery(query);
            }
            else
            {
                if (query.Method == "aggregate")
                {
                    query.RemoveComponent("limit")
                        .RemoveComponent("order")
                        .RemoveComponent("group");

                    query = compiler.TransformAggregateQuery(query);
                }

                ctx = compiler.CompileSelectQuery(query);
            }

            // handle CTEs
            if (query.HasComponent("cte", compiler.EngineCode)) ctx = compiler.CompileCteQuery(ctx, query);

            ctx.RawSql = Helper.ExpandParameters(ctx.RawSql,
                Compiler.ParameterPlaceholder, ctx.Bindings.ToArray());

            return ctx;
        }

        private static SqlResult PrepareResult(this Compiler compiler, SqlResult ctx)
        {
            ctx.NamedBindings = compiler.GenerateNamedBindings(ctx.Bindings.ToArray());
            ctx.Sql = Helper.ReplaceAll(ctx.RawSql,
                Compiler.ParameterPlaceholder, i => compiler.ParameterPrefix + i);
            return ctx;
        }

        private static Dictionary<string, object?> GenerateNamedBindings(this Compiler compiler,object?[] bindings)
        {
            return Helper.Flatten(bindings).Select((v, i) => new { i, v })
                .ToDictionary(x => compiler.ParameterPrefix + x.i, x => x.v);
        }
    }
}
