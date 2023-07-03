using Microsoft.Extensions.DependencyInjection;
using SqlKata.Compilers.DDLCompiler;
using SqlKata.Compilers.DDLCompiler.Abstractions;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.ColumnCompilers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.CreateTableCompilers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.PrimaryKeyCompilers;
using SqlKata.Compilers.DDLCompiler.CreateTableBuilders.UniqueConstraintCompilers;
using SqlKata.Compilers.DDLCompiler.Providers;

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
            services.AddSingleton<ISqlCreateCommandProvider, SqlCommandUtilProvider>();
            services.AddSingleton<ICreateTableFormatFactoryProvider,CreateTableFormatFactoryProvider>();
            services.AddSingleton<ICreateTableQueryFillerProvider,CreateTableFormatFillerProvider>();
            services.AddSingleton<IDDLCompiler, DDLCompiler>();

            return services;
        }
    }
}
