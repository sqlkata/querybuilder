using System;
using System.Reflection;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure;

/// <summary>
///     A test class to expose private methods
/// </summary>
internal class TestCompiler : Compiler
{
    protected TestCompiler()
    {
        EngineCode = "test";
    }

    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}

internal class TestSqlServerCompiler : SqlServerCompiler
{
    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}

internal class TestMySqlCompiler : MySqlCompiler
{
    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}

internal class TestPostgresCompiler : PostgresCompiler
{
    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}

internal class TestFirebirdCompiler : FirebirdCompiler
{
    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}

internal class TestEmptyIdentifiersCompiler : TestCompiler
{
    public TestEmptyIdentifiersCompiler()
    {
        OpeningIdentifier = ClosingIdentifier = "";
    }
}
