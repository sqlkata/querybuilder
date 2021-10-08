using System;
using System.Reflection;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure
{
    /// <summary>
    /// A test class to expose private methods
    /// </summary>
    class TestCompiler : Compiler
    {
        public override string EngineCode { get; } = "test";

        public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return FindCompilerMethodInfo(clauseType, methodName);
        }
    }

    class TestSqlServerCompiler : SqlServerCompiler
    {
        public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return FindCompilerMethodInfo(clauseType, methodName);
        }
    }

    class TestMySqlCompiler : MySqlCompiler
    {
        public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return FindCompilerMethodInfo(clauseType, methodName);
        }
    }

    class TestPostgresCompiler : PostgresCompiler
    {
        public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return FindCompilerMethodInfo(clauseType, methodName);
        }
    }

    class TestFirebirdCompiler : FirebirdCompiler
    {
        public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return FindCompilerMethodInfo(clauseType, methodName);
        }
    }
}
