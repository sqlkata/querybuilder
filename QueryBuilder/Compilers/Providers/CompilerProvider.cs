using System.Collections.Generic;
using System.Linq;
using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.Enums;

namespace SqlKata.Compilers.Providers
{
    internal class CompilerProvider : ICompilerProvider
    {
        private readonly Dictionary<DataSource, ICompilerFactory> _compilerFactoriesByDataSource;

        public CompilerProvider(IEnumerable<ICompilerFactory> compilerFactories)
        {
            _compilerFactoriesByDataSource = compilerFactories.ToDictionary(x => x.DataSource);
        }
        public Compiler CreateCompiler(DataSource dataSource)
        {
            return _compilerFactoriesByDataSource[dataSource].CreateCompiler();
        }
    }
}
