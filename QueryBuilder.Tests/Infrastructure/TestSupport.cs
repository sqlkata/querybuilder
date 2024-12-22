using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure
{
    public abstract class TestSupport
    {
        protected readonly TestCompilersContainer Compilers = new TestCompilersContainer();

        /// <summary>
        /// For legacy test support
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected IReadOnlyDictionary<string, string> Compile(Query query)
        {
            return Compilers.Compile(query).ToDictionary(s => s.Key, v => v.Value.ToString());
        }

        protected SqlResult CompileFor(string engine, Query query)
        {
            var compiler = CreateCompiler(engine);

            return compiler.Compile(query);
        }

        protected SqlResult CompileFor(string engine, Query query, Action<Compiler> action)
        {
            var compiler = CreateCompiler(engine);
            action(compiler);

            return compiler.Compile(query);
        }

        private static Compiler CreateCompiler(string engine)
        {
            return engine switch
            {
                EngineCodes.Firebird => new FirebirdCompiler(),
                EngineCodes.MySql => new MySqlCompiler(),
                EngineCodes.Oracle => new OracleCompiler(),
                EngineCodes.PostgreSql => new PostgresCompiler(),
                EngineCodes.Sqlite => new SqliteCompiler(),
                EngineCodes.SqlServer => new SqlServerCompiler()
                {
                    UseLegacyPagination = true
                },
                _ => throw new ArgumentException($"Unsupported engine type: {engine}", nameof(engine)),
            };
        }
    }
}
