using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using Xunit;

namespace SqlKata.Tests.Snowflake
{
    public class SnowflakeParameterTests : TestSupport
    {
        private readonly SnowflakeCompiler compiler;

        public SnowflakeParameterTests()
        {
            compiler = Compilers.Get<SnowflakeCompiler>(EngineCodes.Snowflake);
        }

        [Fact]
        public void TestDateTimeParameter()
        {
            {
                var dt = new DateTime(1997, 04, 13, 07, 30, 00, DateTimeKind.Utc);
                var query = new Query()
                    .Select("Column")
                    .From("Table")
                    .Where("Column", "<", dt)
                ;
                var ctx = new SqlResult { Query = query };

                Assert.Equal("WHERE \"Column\" < ?", compiler.CompileWheres(ctx));
                Assert.Equal(new List<object>() { "1997-04-13 07:30:00Z" }, ctx.Bindings);
            }

            Assert.IsType<ArgumentException>(
                Assert.Throws<Exception>(() =>
                {
                    var dt = new DateTime(1234, 5, 6, 7, 8, 9, DateTimeKind.Local);
                    var query = new Query()
                        .Select("Column")
                        .From("Table")
                        .Where("Column", "<", dt)
                    ;
                    var ctx = new SqlResult { Query = query };
                    compiler.CompileWheres(ctx);
                })
                .InnerException.InnerException
            );

            Assert.IsType<ArgumentException>(
                Assert.Throws<Exception>(() =>
                {
                    var dt = new DateTime(1234, 5, 6, 7, 8, 9, DateTimeKind.Unspecified);
                    var query = new Query()
                        .Select("Column")
                        .From("Table")
                        .Where("Column", "<", dt)
                    ;
                    var ctx = new SqlResult { Query = query };
                    compiler.CompileWheres(ctx);
                })
                .InnerException.InnerException
            );
        }

        [Fact]
        public void TestDateTimeOffsetParameter()
        {
            {
                var dt = new DateTimeOffset(new DateTime(1234, 5, 6, 8, 10, 9, DateTimeKind.Unspecified), new TimeSpan(1, 2, 0));
                var query = new Query()
                    .Select("Column")
                    .From("Table")
                    .Where("Column", "<", dt)
                ;
                var ctx = new SqlResult { Query = query };

                Assert.Equal("WHERE \"Column\" < ?", compiler.CompileWheres(ctx));
                Assert.Equal(new List<object>() { "1234-05-06 07:08:09Z" }, ctx.Bindings);
            }

            // DateTimeKind.Local offset can vary and so not tested

            {
                var dt = new DateTimeOffset(new DateTime(1234, 5, 6, 7, 8, 9, DateTimeKind.Utc), new TimeSpan(0, 0, 0));
                var query = new Query()
                    .Select("Column")
                    .From("Table")
                    .Where("Column", "<", dt)
                ;
                var ctx = new SqlResult { Query = query };

                Assert.Equal("WHERE \"Column\" < ?", compiler.CompileWheres(ctx));
                Assert.Equal(new List<object>() { "1234-05-06 07:08:09Z" }, ctx.Bindings);
            }
        }
    }
}
