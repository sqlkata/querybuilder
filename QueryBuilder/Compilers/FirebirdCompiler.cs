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

        protected override void CompileInsertQuery(Query query, Writer writer)
        {
            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);
            if (inserts[0] is InsertQueryClause)
            {
                base.CompileInsertQuery(query, writer);
                return;
            }

            var clauses = inserts.Cast<InsertClause>().ToArray();
            if (clauses.Length == 1)
            {
                base.CompileInsertQuery(query, writer);
                return;
            }
            CompileValueInsertClauses(clauses);
            return;

            void CompileValueInsertClauses(InsertClause[] insertClauses)
            {
                var firstInsert = insertClauses.First();

                writer.Append(MultiInsertStartClause);
                writer.Append(" ");
                var table = WriteTable(query, writer, "insert");
                writer.WriteInsertColumnsList(firstInsert.Columns);
                writer.Append(" SELECT ");
                writer.CommaSeparatedParameters(query, firstInsert.Values);
                CompileRemainingInsertClauses(query, table, writer, insertClauses);
            }
        }

        protected override void CompileRemainingInsertClauses(Query query,
            string table, Writer writer, IEnumerable<InsertClause> inserts)
        {
            foreach (var insert in inserts.Skip(1))
            {
                writer.Append(" FROM RDB$DATABASE UNION ALL SELECT ");
                writer.CommaSeparatedParameters(query, insert.Values);
            }
            writer.Append(" FROM RDB$DATABASE");
        }
        protected override string? CompileLimit(Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset > 0)
            {
                writer.Append("ROWS ");
                writer.AppendParameter(offset + 1);
                writer.Append(" TO ");
                writer.AppendParameter(limit + offset);
                return writer;
            }

            return null;
        }


        protected override void CompileColumns(Query query, Writer writer)
        {
            var limit = query.GetLimit(EngineCode);
            var offset = query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                writer.Append("SELECT FIRST ");
                writer.AppendParameter(limit);
                writer.Append(" ");
                CompileColumnsAfterSelect(query, writer);
                return;
            }

            if (limit == 0 && offset > 0)
            {
                writer.Append("SELECT SKIP ");
                writer.AppendParameter(offset);
                writer.Append(" ");
                CompileColumnsAfterSelect(query, writer);
                return;
            }

            base.CompileColumns(query, writer);
        }

        protected override void CompileBasicDateCondition(Query query, BasicDateCondition x, Writer writer)
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
            writer.AppendParameter(query, x.Value);
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
