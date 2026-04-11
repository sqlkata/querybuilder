using System.Reflection;

namespace SqlKata.Tests.Infrastructure.TestCompilers;

class TestFirebirdCompiler : FirebirdCompiler
{
    public virtual MethodInfo Call_FindCompilerMethodInfo(Type clauseType, string methodName)
    {
        return FindCompilerMethodInfo(clauseType, methodName);
    }
}
