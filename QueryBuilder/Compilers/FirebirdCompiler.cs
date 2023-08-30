namespace SqlKata.Compilers
{
    public class FirebirdCompiler : Compiler
    {
        public FirebirdCompiler()
        {
            EngineCode = EngineCodes.Firebird;
            SingleRowDummyTableName = "RDB$DATABASE";
            XService = new("\"", "\"", "AS ", true);
        }

        public override void CompileInsertQuery(SqlResult ctx, Query query, Writer writer)
        {
           // var fromClause = GetFromClause(query, EngineCode);
           // var table = GetTable(ctx, fromClause, XService);
            writer.AssertMatches(ctx);


            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            if (inserts[0] is InsertQueryClause)
            {
                base.CompileInsertQuery(ctx, query, writer);
                return;
            }

            var clauses = inserts.Cast<InsertClause>().ToArray();
            if (clauses.Length == 1)
            {
                base.CompileInsertQuery(ctx, query, writer);
                return;
            }
            CompileValueInsertClauses(clauses);
            writer.AssertMatches(ctx);
            return;

            void CompileValueInsertClauses(InsertClause[] insertClauses)
            {
                var firstInsert = insertClauses.First();

                writer.Append(MultiInsertStartClause);
                writer.Append(" ");
                var table = WriteTable(ctx, query.GetOneComponent<AbstractFrom>("from", EngineCode), writer, "insert");
                writer.WriteInsertColumnsList(firstInsert.Columns);
                writer.Append(" SELECT ");
                writer.List(", ", firstInsert.Values, p =>
                {
                    writer.Append(Parameter(ctx, query, writer, p));
                });

                CompileRemainingInsertClauses(ctx, query, table, writer, insertClauses);
                ctx.Raw.Append(writer);
            }
        }

        protected override void CompileRemainingInsertClauses(SqlResult ctx, Query query, string table,
            Writer writer,
            IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                writer.Append(" FROM RDB$DATABASE UNION ALL SELECT ");
                writer.List(", ", insert.Values, value =>
                {
                    writer.Append(Parameter(ctx, query, writer, value));
                });
            }
            writer.Append(" FROM RDB$DATABASE");
        }
        protected override string? CompileLimit(SqlResult ctx, Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                ctx.BindingsAdd(offset + 1);
                ctx.BindingsAdd(limit + offset);

                writer.Append("ROWS ? TO ?");
                return writer;
            }

            return null;
        }


        protected override string CompileColumns(SqlResult ctx, Query query, Writer writer)
        {
            var compiled = base.CompileColumns(ctx, query, writer.Sub());

            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                ctx.PrependOne(limit);

                query.RemoveComponent("limit");

                writer.Append("SELECT FIRST ?");
                writer.Append(compiled.Substring(6));
                return writer;
            }

            if (limit == 0 && offset > 0)
            {
                ctx.PrependOne(offset);

                query.RemoveComponent("offset");

                writer.Append("SELECT SKIP ?");
                writer.Append(compiled.Substring(6));
                return writer;
            }

            writer.Append(compiled);
            return writer;
        }

        protected override void CompileBasicDateCondition(SqlResult ctx, Query query, BasicDateCondition x,
            Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT (");
            if (x.Part == "time")
            {
                writer.Append("CAST(");
                writer.AppendName(x.Column);
                writer.Append(" as TIME) ");
            }
            else if (x.Part == "date")
            {
                writer.Append("CAST(");
                writer.AppendName(x.Column);
                writer.Append(" as DATE) ");
            }
            else
            {
                writer.Append("EXTRACT(");
                writer.AppendName(x.Part.ToUpperInvariant());
                writer.Append(" FROM ");
                writer.AppendName(x.Column);
                writer.Append(") ");
            }
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.Append(Parameter(ctx, query, writer, x.Value));
            if (x.IsNot)
                writer.Append(")");
        }


        protected override string CompileTrue()
        {
            return "1";
        }

        protected override string CompileFalse()
        {
            return "0";
        }
    }
}
