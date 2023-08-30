using System.Collections.Immutable;

namespace SqlKata.Compilers
{
    public static class CompilationExtensions
    {
        public static SqlResult Compile(this Compiler compiler, Query query)
        {
            var writer = new Writer(compiler.XService);
            var ctx = compiler.CompileRaw(query, writer);
            writer.AssertMatches(ctx);
            ctx = compiler.PrepareResult(ctx);
            return ctx;
        }

        public static SqlResult Compile(this Compiler compiler, IEnumerable<Query> queries)
        {
            var compiled = queries.Select(x=> compiler
                .CompileRaw(x, new Writer(compiler.XService)))
                .ToArray();
            var combinedBindings = compiled.SelectMany(r => r.Bindings).ToList();
            var ctx = new SqlResult(combinedBindings);
            
            ctx.Raw.Append(compiled.Select(r => r.RawSql)
                .Aggregate((a, b) => a + ";\n" + b));

            ctx = compiler.PrepareResult(ctx);

            return ctx;
        }

        private static SqlResult CompileRaw(this Compiler compiler, Query query, Writer writer)
        {
            var ctx = new SqlResult();
            writer.Push(ctx);
            // handle CTEs
            if (query.HasComponent("cte", compiler.EngineCode))
            {
                compiler.CompileCteQuery(query, writer);
                ctx.BindingsAddRange(writer.Bindings);
            }
            if (query.Method == "insert")
            {
                
                compiler.CompileInsertQuery(ctx, query, writer);
            }
            else if (query.Method == "update")
            {
              
                compiler.CompileUpdateQuery(ctx, query, writer);
            }
            else if (query.Method == "delete")
            {
             
                compiler.CompileDeleteQuery(ctx, query, writer);
            }
            else
            {
                if (query.Method == "aggregate")
                {
                    query.RemoveComponent("limit")
                        .RemoveComponent("order")
                        .RemoveComponent("group");

                    query = TransformAggregateQuery(query);
                }
              
                compiler.CompileSelectQueryInner(ctx, query, writer);
            }


            // "... WHERE `Id` in (?)" -> "... WHERE `Id` in (?,?,?)"
            ctx.ReplaceRaw(BindingExtensions.ExpandParameters(ctx.RawSql,
                "?", ctx.Bindings.ToArray()));

            return ctx;
            Query TransformAggregateQuery(Query query1)
            {
                var clause = query1.GetOneComponent<AggregateClause>("aggregate", compiler.EngineCode)!;

                if (clause.Columns.Length == 1 && !query1.IsDistinct) return query1;

                if (query1.IsDistinct)
                {
                    query1.RemoveComponent("aggregate", compiler.EngineCode);
                    query1.RemoveComponent("select", compiler.EngineCode);
                    query1.Select(clause.Columns.ToArray());
                }
                else
                {
                    foreach (var column in clause.Columns) query1.WhereNotNull(column);
                }

                var outerClause = new AggregateClause
                {
                    Engine = compiler.EngineCode,
                    Component = "aggregate",
                    Columns = ImmutableArray.Create<string>().Add("*"),
                    Type = clause.Type
                };

                return new Query()
                    .AddComponent(outerClause)
                    .From(query1, $"{clause.Type}Query");
            }

        }

        private static SqlResult PrepareResult(this Compiler compiler, SqlResult ctx)
        {
            ctx.NamedBindings = ctx.Bindings.GenerateNamedBindings(compiler.ParameterPrefix);
            ctx.Sql = BindingExtensions.ReplaceAll(ctx.RawSql,
                "?", i => compiler.ParameterPrefix + i);
            return ctx;
        }


    }
}
