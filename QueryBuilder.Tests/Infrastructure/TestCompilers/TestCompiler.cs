using System;
using System.Reflection;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure.TestCompilers
{
    /// <summary>
    /// A test class to expose private methods
    /// </summary>
    class TestCompiler : Compiler
    {
        public override string EngineCode { get; } = "generic";

        public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
        {
            return FindCompilerMethodInfo(clauseType, methodName);
        }
    }
}

