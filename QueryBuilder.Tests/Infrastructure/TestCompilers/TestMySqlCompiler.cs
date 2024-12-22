using System;
using System.Reflection;
using SqlKata.Compilers;

namespace SqlKata.Tests.Infrastructure.TestCompilers;

class TestMySqlCompiler : MySqlCompiler
{
    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}
