using API.Services;
using Database;
using Domain.Users.Users.Interface;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Interfaces;
using Shared.Infra;

namespace API.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext with Scoped lifetime
            services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(configuration.GetConnectionString("ManagementConnection"));
                }
            );

            services.AddScoped<Func<AppDbContext>>((provider) => () => provider.GetService<AppDbContext>());
            services.AddScoped<DbFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IUserRepository, UserRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddScoped<UserService>();
        }
    }
}
