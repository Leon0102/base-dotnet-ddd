using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Controllers;

namespace Shared.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
            });
            return services;
        }

        public static IServiceCollection AddDatabaseContext<T>(this IServiceCollection services, IConfiguration config) where T : DbContext
        {
            var connectionString = config.GetConnectionString("Default");
            services.AddPostgresQL<T>(connectionString ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres");
            return services;
        }

        private static IServiceCollection AddPostgresQL<T>(this IServiceCollection services, string connectionString) where T : DbContext
        {
            services.AddDbContext<T>(m => m.UseNpgsql(connectionString, e => e.MigrationsAssembly(typeof(T).Assembly.FullName)));
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            dbContext.Database.Migrate();
            return services;
        }
    }
}