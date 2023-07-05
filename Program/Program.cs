using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.DBSpecificQueries;
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
            var compiler = compilerProvider.CreateCompiler(DataSource.Postgresql);

            //var query = new Query("Users").DropTable();
            //var query = new Query("Users").Truncate();
            var query = CreateTable();
            //var query = CreateTableAs();
            Console.WriteLine(compiler.Compile(query));



        }

        private static Query CreateTableAs()
        {
            var selectQuery = new Query("Users").Select("id", "fullname", "age");
            var query = new Query("SampleUsers").CreateTableAs(selectQuery, TableType.Temporary,
                new OracleDbTableExtensions() { OnCommitPreserveRows = true });
            return query;
        }

        private static Query CreateTable()
        {
            var query = new Query("Users").CreateTable(new List<TableColumnDefinitionDto>()
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
                        Length = 30,
                        //Collation = "Arabiv_Ci_100_Ai"
                    },
                    IsAutoIncrement = false,
                    IsNullable = false,
                    IsPrimaryKey = false,
                    IsUnique = true,
                }
            }, TableType.Temporary);
            return query;
        }
    }
}
