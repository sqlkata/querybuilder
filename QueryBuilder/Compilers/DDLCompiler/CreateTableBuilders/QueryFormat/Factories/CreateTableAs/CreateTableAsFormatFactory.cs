using SqlKata.Compilers.DDLCompiler.Abstractions;

namespace SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories.CreateTableAs
{
    public class CreateTableAsFormatFactory : ICreateTableAsFormatFactory
    {
        public string MakeCreateTableAsFormat()
        {
            return @"CREATE {0} TABLE {1} {2}
As ({3}) ";
        }
    }
}
