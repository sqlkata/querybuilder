using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata.Compilers
{
    internal class ConditionsCompilerProvider
    {
        private readonly Type _compilerType;
        private readonly Dictionary<string, MethodInfo> _methodsCache = new Dictionary<string, MethodInfo>();
        private readonly object _syncRoot = new object();

        public ConditionsCompilerProvider(Compiler compiler)
        {
            _compilerType = compiler.GetType();
        }

        public MethodInfo GetMethodInfo(Type clauseType, string methodName)
        {
            // The cache key should take the type and the method name into consideration
            var cacheKey = methodName + "::" + clauseType.FullName;

            lock (_syncRoot)
            {
                if (_methodsCache.TryGetValue(cacheKey, out var info)) return info;

                return _methodsCache[cacheKey] = FindMethodInfo(clauseType, methodName);
            }
        }

        private MethodInfo FindMethodInfo(Type clauseType, string methodName)
        {
            var methodInfo = _compilerType
                .GetRuntimeMethods()
                .FirstOrDefault(x => x.Name == methodName);

            if (methodInfo == null) throw new Exception($"Failed to locate a compiler for '{methodName}'.");

            if (clauseType.IsConstructedGenericType && methodInfo.GetGenericArguments().Any())
                methodInfo = methodInfo.MakeGenericMethod(clauseType.GenericTypeArguments);

            return methodInfo;
        }
    }
}
