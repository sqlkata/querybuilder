using Microsoft.Extensions.DependencyInjection;
using SqlKata.Compilers.Abstractions;
using SqlKata.Compilers.DDLCompiler;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.ColumnCompilers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableCompilers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableQueryUtils;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.DBSpecificQueries;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.PrimaryKeyCompilers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories.CreateTable;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Factories.CreateTableAs;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers.CreateTable;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.QueryFormat.Fillers.CreateTableAs;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.UniqueConstraintCompilers;
using SqlKata.Compilers.DDLCompiler.DeleteDdl;
using SqlKata.Compilers.DDLCompiler.Providers;
using SqlKata.Compilers.Providers;
using SqlKata.Compilers.Providers.Factories;

namespace SqlKata
{
    public static class KataDependencyInjection
    {
        public static IServiceCollection AddKataServices(this IServiceCollection services)
        {
            services.AddSingleton<IColumnCompiler, ColumnCompiler>();
            services.AddSingleton<IPrimaryKeyCompiler, PrimaryKeyCompiler>();
            services.AddSingleton<IUniqueConstraintCompiler, UniqueConstraintCompiler>();
            services.AddSingleton<ICreateTableQueryCompiler,CreateTableCompiler>();
            services.AddSingleton<ICreateTableAsCompiler, CreateTableAsCompiler>();
            services.AddSingleton<ISqlCreateCommandProvider, SqlCommandUtilProvider>();
            services.AddSingleton<ICreateTableFormatFactoryProvider,CreateTableFormatFactoryProvider>();
            services.AddSingleton<ICreateTableQueryFillerProvider,CreateTableFormatFillerProvider>();
            services.AddSingleton<IDDLCompiler, DDLCompiler>();

            services.AddSingleton<ISqlCreateCommandUtil, SqlServerCreateCommandUtil>();
            services.AddSingleton<ISqlCreateCommandUtil, MySqlCreateCommandUtil>();
            services.AddSingleton<ISqlCreateCommandUtil, PostgresqlCreateCommandUtil>();
            services.AddSingleton<ISqlCreateCommandUtil, OracleCreateCommandUtil>();

            services.AddSingleton<ICreateQueryFormatFactory,SqlServerCreateTableFormatFactory>();
            services.AddSingleton<ICreateQueryFormatFactory,MySqlCreateTableFormatFactory>();
            services.AddSingleton<ICreateQueryFormatFactory,PostgresqlCreateTableFormatFactory>();
            services.AddSingleton<ICreateQueryFormatFactory,OracleCreateTableFormatFactory>();

            services.AddSingleton<ICreateQueryFormatFiller,SqlServerCreateQueryFormatFiller>();
            services.AddSingleton<ICreateQueryFormatFiller,PostgresqlCreateQueryFormatFiller>();
            services.AddSingleton<ICreateQueryFormatFiller,MySqlCreateQueryFormatFiller>();
            services.AddSingleton<ICreateQueryFormatFiller,OracleCreateQueryFormatFiller>();

            services.AddSingleton<ICompilerFactory,SqlServerCompilerFactory>();
            services.AddSingleton<ICompilerFactory,OracleCompilerFactory>();
            services.AddSingleton<ICompilerFactory,PostgresCompilerFactory>();
            services.AddSingleton<ICompilerFactory,MySqlCompilerFactory>();
            services.AddSingleton<ICompilerFactory,FirebirdCompilerFactory>();
            services.AddSingleton<ICompilerFactory,SqliteCompilerFactory>();

            services.AddSingleton<ICreateTableAsFormatFactory,CreateTableAsFormatFactory>();
            services.AddSingleton<ICreateTableAsFormatFiller,CreateTableAsFormatFiller>();

            services.AddSingleton<IOracleCreateTableDbExtender,OracleCreateTableDbExtender>();

            services.AddSingleton<ICompilerProvider,CompilerProvider>();

            services.AddSingleton<IDropTableQueryFactory, DropTableQueryFactory>();
            services.AddSingleton<ITruncateTableQueryFactory, TruncateTableQueryFactory>();
            
            return services;
        }
    }
}
