using System;
using System.Collections.Generic;
using SqlKata;
using SqlKata.Compilers;

namespace Program
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var columns = new List<string> {"Test1", "Test2"};

            var query = new Query().Select("1", "2", "3", "4").From(new Query(),);

            //var query = new Query("ErrorLogs")
            //    .AsDeleteRaw("TRUNCAT TABLE [ErrorLogs]");

            var compiler = new SqlServerCompiler();
            var sql = compiler.Compile(query);

            Console.WriteLine(sql);
            Console.WriteLine(string.Join(", ", compiler.GetBindings()));
        }
    }
}