using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure
{
    public class TestCompilersContainer
    {
        protected readonly IDictionary<string, Compiler> Compilers = new Dictionary<string, Compiler>
        {
            [EngineCodes.Firebird] = new FirebirdCompiler(),
            [EngineCodes.MySql] = new MySqlCompiler(),
            [EngineCodes.Oracle] = new OracleCompiler(),
            [EngineCodes.PostgreSql] = new PostgresCompiler(),
            [EngineCodes.Sqlite] = new SqliteCompiler(),
            [EngineCodes.SqlServer] = new SqlServerCompiler()
        };

        /// <summary>
        /// Returns the compiler instance for the given <param name="engineCode"></param>
        /// </summary>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Compiler Get(string engineCode)
        {
            if (!Compilers.ContainsKey(engineCode))
            {
                throw new InvalidOperationException($"Engine code '{engineCode}' is not valid");
            }

            return Compilers[engineCode];
        }

        /// <summary>
        /// Convenience method <seealso cref="Get"/>
        /// </summary>
        /// <remarks>Does not validate generic type against engine code before cast</remarks>
        /// <typeparam name="TCompiler"></typeparam>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public TCompiler Get<TCompiler>(string engineCode) where TCompiler : Compiler
        {
            return (TCompiler) Get(engineCode);
        }

        /// <summary>
        /// Compiles the query for the given <param name="engineCode"></param>
        /// </summary>
        /// <param name="engineCode"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public SqlResult CompileFor(string engineCode, Query query)
        {
            var compiler = Get(engineCode);
            return compiler.Compile(query);
        }

        /// <summary>
        /// Compiles the query with all enabled compilers.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public TestSqlResultContainer Compile(Query query)
        {
            var resultKeyValues = Compilers
                .ToDictionary(k => k.Key, v => v.Value.Compile(query.Clone()));
            return new TestSqlResultContainer(resultKeyValues);
        }
    }
}