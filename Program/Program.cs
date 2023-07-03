using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;
using SqlKata.DbTypes.DbColumn;
using SqlKata.DbTypes.Enums;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKataServices();
            var provider = serviceCollection.BuildServiceProvider();
            var compilerProvider = provider.GetRequiredService<ICompilerProvider>();
            var compiler = compilerProvider.CreateCompiler(DataSource.Postgresql);

            var query = new Query("Users").CreateTable(new List<TableColumnDefenitionDto>()
            {
                new TableColumnDefenitionDto()
                {
                    ColumnName = "id",
                    ColumnDbType = new PostgresqlDBColumn()
                    {
                        PostgresqlDbType = PostgresqlDbType.Integer
                    },
                    IsAutoIncrement = true,
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsUnique = false,
                },
                new TableColumnDefenitionDto()
                {
                    ColumnName = "FullName",
                    ColumnDbType = new PostgresqlDBColumn()
                    {
                        PostgresqlDbType = PostgresqlDbType.Character_varying,
                        Length = 30
                    },
                    IsAutoIncrement = false,
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsUnique = true,
                }
            }, TableType.Temporary);

            Console.WriteLine(compiler.Compile(query));



        }
    }
}
