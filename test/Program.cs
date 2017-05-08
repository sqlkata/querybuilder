using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SqlKata;
using SqlKata.Compilers;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {

            var query = new Query(new SqlServerCompiler());

            var filterByGender = true;

            query.Select("name", "gender", "classification")
                .From("People")
                .WhereLike("name", "AhmAd")


                .ForPage(1, 10)
                // .Limit(10)
                .Where("gender", "M")
;



            LogQuery(query);


            /*
            Console.WriteLine(typeof(string).Name); // String
            Console.WriteLine(typeof(String).Name); // String 
            Console.WriteLine(typeof(Int16).Name); // Int16
            Console.WriteLine(typeof(int).Name); // Int32
            Console.WriteLine(typeof(Int32).Name); // Int32
            Console.WriteLine(typeof(Int64).Name); // Int64
            Console.WriteLine(typeof(long).Name); // Int64
            Console.WriteLine(typeof(double).Name); // Double
            Console.WriteLine(typeof(float).Name); // Single
            Console.WriteLine(typeof(decimal).Name); // Decimal
            Console.WriteLine(typeof(DateTime).Name); // DateTime


            A i = new B<string>();
            i.Value = "jkj sd";

            parse(i);

            */


        }

        public static void LogQuery(Query query)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            var result = query.ToSql(format: false);
            Console.WriteLine(result.Sql);
            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (result.Bindings.Any())
            {
                Console.WriteLine(string.Join(", ", result.Bindings));
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        public static void parse(A a)
        {
            Console.WriteLine(a.GetType().GenericTypeArguments.First().Name);
            Console.WriteLine(a.Value as string);
        }

    }
    class A
    {
        public object Value { get; set; }
    }

    class B<T> : A
    {
        public new T Value { get; set; }
    }
}
