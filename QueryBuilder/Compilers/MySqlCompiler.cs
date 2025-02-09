using SqlKata.Clauses;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers
{
    public class MySqlCompiler : Compiler
    {
        public MySqlCompiler(IDDLCompiler ddlCompiler) : this()
        {
            DdlCompiler = ddlCompiler;
        }

        public MySqlCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "`";
            LastId = "SELECT last_insert_id() as Id";
        }


        public override string EngineCode { get; } = EngineCodes.MySql;

        public override string CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);


            if (offset == 0 && limit == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return $"LIMIT {parameterPlaceholder}";
            }

            if (limit == 0)
            {

                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.Bindings.Add(offset);
                return $"LIMIT 18446744073709551615 OFFSET {parameterPlaceholder}";
            }

            // We have both values

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return $"LIMIT {parameterPlaceholder} OFFSET {parameterPlaceholder}";

        }

        protected override SqlResult CompileCreateTableAs(Query query)
        {
            var compiledSelectQuery = CompileSelectQuery(query.GetOneComponent<CreateTableAsClause>("CreateTableAsQuery").SelectQuery).RawSql;
            return DdlCompiler.CompileCreateTableAs(query,DataSource.MySql,compiledSelectQuery);
        }

        protected override SqlResult CompileCreateTable(Query query)
        {
            return DdlCompiler.CompileCreateTable(query,DataSource.MySql);
        }
    }
}
