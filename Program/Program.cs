using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SqlKata;
using SqlKata.Compilers;
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
            var provider = serviceCollection.BuildServiceProvider();


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
            var compiler = new PostgresCompiler();
            Console.WriteLine(compiler.Compile(query));



        }
    }
}
