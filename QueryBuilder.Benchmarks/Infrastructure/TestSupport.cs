using SqlKata;
using SqlKata.Compilers;

namespace QueryBuilder.Benchmarks.Infrastructure;

public class TestSupport
{

    public static SqlResult CompileFor(string engine, Query query, Func<Compiler, Compiler> configuration = null)
    {
        var compiler = CreateCompiler(engine);
        if (configuration != null)
        {
            compiler = configuration(compiler);
        }

        return compiler.Compile(query);
    }

    public static SqlResult CompileFor(string engine, Query query, Action<Compiler> configuration)
    {
        return CompileFor(engine, query, compiler =>
        {
            configuration(compiler);
            return compiler;
        });
    }

    public static Compiler CreateCompiler(string engine)
    {
        return engine switch
        {
            EngineCodes.Firebird => new FirebirdCompiler(),
            EngineCodes.MySql => new MySqlCompiler(),
            EngineCodes.Oracle => new OracleCompiler
            {
                UseLegacyPagination = false
            },
            EngineCodes.PostgreSql => new PostgresCompiler(),
            EngineCodes.Sqlite => new SqliteCompiler(),
            EngineCodes.SqlServer => new SqlServerCompiler
            {
                UseLegacyPagination = false
            },
            EngineCodes.Generic => new TestCompiler(),
            _ => throw new ArgumentException($"Unsupported engine type: {engine}", nameof(engine)),
        };
    }
}
