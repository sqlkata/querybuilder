﻿using SqlKata;
using SqlKata.Compilers;
using Xunit;

namespace QueryBuilder.Tests
{
    public class CompilerTest
    {
        private readonly Compiler pgsql;
        private readonly MySqlCompiler mysql;
        private readonly FirebirdCompiler fbsql;
        private SqlServerCompiler mssql { get; }

        public CompilerTest()
        {
            mssql = new SqlServerCompiler();
            mysql = new MySqlCompiler();
            pgsql = new PostgresCompiler();
            fbsql = new FirebirdCompiler();
        }

        private string[] Compile(Query q)
        {
            return new[]{
                mssql.Compile(q.Clone()).ToString(),
                mysql.Compile(q.Clone()).ToString(),
                pgsql.Compile(q.Clone()).ToString(),
                fbsql.Compile(q.Clone()).ToString(),
            };
        }

    }
}
