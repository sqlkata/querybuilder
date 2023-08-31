using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure;

public class TestCompilersContainer
{
    private readonly IDictionary<string, Compiler> _compilers = new Dictionary<string, Compiler>
    {
        [EngineCodes.MySql] = new MySqlCompiler(),
        [EngineCodes.Oracle] = new OracleCompiler(),
        [EngineCodes.PostgreSql] = new PostgresCompiler(),
        [EngineCodes.Sqlite] = new SqliteCompiler(),
        [EngineCodes.SqlServer] = new SqlServerCompiler
        {
            UseLegacyPagination = true
        },
        [EngineCodes.Firebird] = new FirebirdCompiler(),
    };

    public ICollection<string> KnownEngineCodes
    {
        get { return _compilers.Keys; }
    }

    /// <summary>
    ///     Returns a <see cref="Compiler" /> instance for the given engine code
    /// </summary>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public Compiler Get(string engineCode)
    {
        if (!_compilers.ContainsKey(engineCode))
            throw new InvalidOperationException(string.Format(Messages.ErrInvalidEngineCode, engineCode));

        return _compilers[engineCode];
    }

    /// <summary>
    ///     Convenience method <seealso cref="Get" />
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
    ///     Compiles the <see cref="Query" /> against the given engine code
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
    ///     Compiles the <see cref="Query" /> against the given engine codes
    /// </summary>
    /// <param name="engineCodes"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public TestSqlResultContainer Compile(IEnumerable<string> engineCodes, Query query)
    {
        var codes = engineCodes.ToList();

        var results = _compilers
            .Where(w => codes.Contains(w.Key))
            .ToDictionary(k => k.Key, v => v.Value.Compile(query.Clone()));

        if (results.Count != codes.Count)
        {
            var missingCodes = codes.Where(w => _compilers.All(a => a.Key != w));
            var templateArg = string.Join(", ", missingCodes);
            throw new InvalidOperationException(string.Format(Messages.ErrInvalidEngineCodes, templateArg));
        }

        return new TestSqlResultContainer(results);
    }

    /// <summary>
    ///     Compiles the <see cref="Query" /> against all <see cref="Compiler" />s
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public TestSqlResultContainer Compile(Query query)
    {
        var resultKeyValues = _compilers
            .ToDictionary(k => k.Key, v => v.Value.Compile(query.Clone()));
        return new TestSqlResultContainer(resultKeyValues);
    }

    private static class Messages
    {
        public const string ErrInvalidEngineCode = "Engine code '{0}' is not valid";
        public const string ErrInvalidEngineCodes = "Invalid engine codes supplied '{0}'";
    }
}
