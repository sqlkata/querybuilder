using System;
using SqlKata;
using SqlKata.Compilers;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var query = new Query("table", "nolock")
            .WhereLike("name", "a")
            .Limit(10).Offset(5)
            .ForPage(3, 4);


            var compiler = new SqlServerCompiler();


            var sql = compiler.Compile(query);


            Console.WriteLine(sql);
            Console.WriteLine(string.Join(", ", compiler.GetBindings()));


        }
    }
}
