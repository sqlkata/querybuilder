using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure
{
    public class TestCompilersContainer
    {
        private static class Messages
        {
            public const string ERR_INVALID_ENGINECODE = "Engine code '{0}' is not valid";
            public const string ERR_INVALID_ENGINECODES = "Invalid engine codes supplied '{0}'";
        }

        protected readonly IDictionary<string, Compiler> Compilers = new Dictionary<string, Compiler>
        {
            [EngineCodes.Firebird] = new FirebirdCompiler(),
            [EngineCodes.MySql] = new MySqlCompiler(),
            [EngineCodes.Oracle] = new OracleCompiler(),
            [EngineCodes.PostgreSql] = new PostgresCompiler(),
            [EngineCodes.Sqlite] = new SqliteCompiler(),
            [EngineCodes.SqlServer] = new SqlServerCompiler()
        };

        public IEnumerable<string> KnownEngineCodes
        {
            get { return Compilers.Select(s => s.Key); }
        }

        /// <summary>
        /// Returns a <see cref="Compiler"/> instance for the given engine code
        /// </summary>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Compiler Get(string engineCode)
        {
            if (!Compilers.ContainsKey(engineCode))
            {
                throw new InvalidOperationException(string.Format(Messages.ERR_INVALID_ENGINECODE, engineCode));
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
            return (TCompiler)Get(engineCode);
        }

        /// <summary>
        /// Compiles the <see cref="Query"/> against the given engine code
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
        /// Compiles the <see cref="Query"/> against the given engine codes
        /// </summary>
        /// <param name="engineCodes"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public TestSqlResultContainer Compile(IEnumerable<string> engineCodes, Query query)
        {
            var codes = engineCodes.ToList();

            var results = Compilers
                .Where(w => codes.Contains(w.Key))
                .ToDictionary(k => k.Key, v => v.Value.Compile(query.Clone()));

            if (results.Count != codes.Count)
            {
                var missingCodes = codes.Where(w => Compilers.All(a => a.Key != w));
                var templateArg = string.Join(", ", missingCodes);
                throw new InvalidOperationException(string.Format(Messages.ERR_INVALID_ENGINECODES, templateArg));
            }

            return new TestSqlResultContainer(results);
        }

        /// <summary>
        /// Compiles the <see cref="Query"/> against all <see cref="Compiler"/>s
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
