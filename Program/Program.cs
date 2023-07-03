using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.Enums;
using SqlKata.Contract.CreateTable;
using SqlKata.Contract.CreateTable.DbTableSpecific;
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
            var compiler = compilerProvider.CreateCompiler(DataSource.Oracle);

            var query = new Query("Users").CreateTable(new List<TableColumnDefenitionDto>()
            {
                new()
                {
                    ColumnName = "id",
                    ColumnDbType = new OracleDBColumn()
                    {
                        OracleDbType = OracleDbType.Int32
                    },
                    IsAutoIncrement = true,
                    IsNullable = false,
                    IsPrimaryKey = true,
                    IsUnique = false,
                },
                new()
                {
                    ColumnName = "FullName",
                    ColumnDbType = new OracleDBColumn()
                    {
                        OracleDbType = OracleDbType.Varchar2,
                        Length = 30
                    },
                    IsAutoIncrement = false,
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsUnique = true,
                }
            }, TableType.Temporary,new OracleDbTableExtensions(){OnCommitPreserveRows = true});

            Console.WriteLine(compiler.Compile(query));



        }
    }
}
