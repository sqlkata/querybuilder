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

        protected override SqlResult CompileCreateTableAs(Query query)
        {
            throw new NotImplementedException();
        }

        protected override SqlResult CompileCreateTable(Query query)
        {
            throw new NotImplementedException();
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

    class TestEmptyIdentifiersCompiler : TestCompiler
    {
        protected override string OpeningIdentifier { get; set; } = "";
        protected override string ClosingIdentifier { get; set; } = "";
    }
}

