using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure.TestCompilers;

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

        protected SqlResult CompileForGeneric(Query query, Func<Compiler, Compiler> configuration = null)
        {
            return CompileFor(EngineCodes.Generic, query, configuration);
        }

        protected SqlResult CompileForGeneric(Query query, Action<Compiler> configuration)
        {
            return CompileFor(EngineCodes.Generic, query, configuration);
        }

        protected SqlResult CompileFor(string engine, Query query, Func<Compiler, Compiler> configuration = null)
        {
            var compiler = CreateCompiler(engine);
            if (configuration != null)
            {
                compiler = configuration(compiler);
            }

            return compiler.Compile(query);
        }

        protected SqlResult CompileFor(string engine, Query query, Action<Compiler> configuration)
        {
            return CompileFor(engine, query, compiler =>
            {
                configuration(compiler);
                return compiler;
            });
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
                EngineCodes.SqlServer => new SqlServerCompiler
                {
                    UseLegacyPagination = true
                },
                EngineCodes.Generic => new TestCompiler(),
                _ => throw new ArgumentException($"Unsupported engine type: {engine}", nameof(engine)),
            };
        }
    }
}
