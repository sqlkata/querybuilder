using System;
using System.Linq;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public class InfrastructureTests : TestSupport
    {
        [Fact]
        public void CanGetCompiler()
        {
            var compiler = Compilers.Get(EngineCodes.SqlServer);

            Assert.NotNull(compiler);
            Assert.IsType<SqlServerCompiler>(compiler);
        }

        [Fact]
        public void CanCompile()
        {
            var results = Compilers.Compile(new Query("Table"));

            Assert.NotNull(results);
            Assert.Equal(Compilers.KnownEngineCodes.Count(), results.Count);
        }

        [Fact]
        public void CanCompileSelectively()
        {
            var desiredEngines = new[] {EngineCodes.SqlServer, EngineCodes.MySql};
            var results = Compilers.Compile(desiredEngines, new Query("Table"));

            Assert.Equal(desiredEngines.Length, results.Count);
            Assert.Contains(results, a =>a.Key == EngineCodes.SqlServer);
            Assert.Contains(results, a => a.Key == EngineCodes.MySql);
        }


        [Fact]
        public void ShouldThrowIfInvalidEngineCode()
        {
            Assert.Throws<InvalidOperationException>(() => Compilers.CompileFor("XYZ", new Query()));
        }

        [Fact]
        public void ShouldThrowIfAnyEngineCodesAreInvalid()
        {
            var codes = new[] { EngineCodes.SqlServer, "123", EngineCodes.MySql, "abc" };
            Assert.Throws<InvalidOperationException>(() => Compilers.Compile(codes, new Query()));
        }
    }
}
