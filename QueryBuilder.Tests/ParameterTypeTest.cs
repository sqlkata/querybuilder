using System;
using System.Collections.Generic;
using SqlKata.Execution;
using SqlKata;
using SqlKata.Compilers;
using Xunit;
using System.Collections;

public enum EnumExample
{
    First,
    Second,
    Third,
}

public class ParameterTypeTest
{
    private readonly Compiler pgsql;
    private readonly MySqlCompiler mysql;
    private readonly FirebirdCompiler fbsql;
    public SqlServerCompiler mssql { get; private set; }

    public ParameterTypeTest()
    {
        mssql = new SqlServerCompiler();
        mysql = new MySqlCompiler();
        pgsql = new PostgresCompiler();
        fbsql = new FirebirdCompiler();
    }

    private string[] Compile(Query q)
    {
        return new[] {
            mssql.Compile(q.Clone()).ToString(),
            mysql.Compile(q.Clone()).ToString(),
            pgsql.Compile(q.Clone()).ToString(),
            fbsql.Compile(q.Clone()).ToString(),
        };
    }
    public class ParameterTypeGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] {"1", 1},
            new object[] {"10.5", 10.5},
            new object[] {"-2", -2},
            new object[] {"-2.8", -2.8},
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

        Assert.Equal($"SELECT * FROM [Table] WHERE [Col] = {rendered}", c[0]);
    }
}
