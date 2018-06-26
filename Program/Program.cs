using System;
using SqlKata;
using SqlKata.Compilers;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var query = new Query("table", "nolock").SelectRaw("DISTINCT CONVERT(varchar(10), [CreateDate], 20) as CreateDate")
                .Where("name", "a");
            
            var compiler = new SqlServerCompiler();


            var sql = compiler.Compile(query);


            Console.WriteLine(sql);
            Console.WriteLine(string.Join(", ", compiler.GetBindings()));


        }
    }
}
