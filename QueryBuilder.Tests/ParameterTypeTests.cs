using System;
using System.Collections.Generic;
using System.Globalization;
using SqlKata.Compilers;
using Xunit;
using System.Collections;
using SqlKata.Tests.Infrastructure;

namespace SqlKata.Tests
{
    public class ParameterTypeTests : TestSupport
    {
        public enum EnumExample
        {
            First,
            Second,
            Third,
        }

        public class ParameterTypeGenerator : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] {"1", 1},
                new object[] {Convert.ToSingle("10.5", CultureInfo.InvariantCulture).ToString(), 10.5},
                new object[] {"-2", -2},
                new object[] {Convert.ToSingle("-2.8", CultureInfo.InvariantCulture).ToString(), -2.8},
                new object[] {"true", true},
                new object[] {"false", false},
                new object[] {"'2018-10-28 19:22:00'", new DateTime(2018, 10, 28, 19, 22, 0)},
                new object[] {"0 /* First */", EnumExample.First},
                new object[] {"1 /* Second */", EnumExample.Second},
                new object[] {"'a string'", "a string"},
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(ParameterTypeGenerator))]
        public void CorrectParameterTypeOutput(string rendered, object input)
        {
            var query = new Query("Table").Where("Col", input);

            var c = Compile(query);

            Assert.Equal($"SELECT * FROM [Table] WHERE [Col] = {rendered}", c[EngineCodes.SqlServer]);
        }
    }
}
