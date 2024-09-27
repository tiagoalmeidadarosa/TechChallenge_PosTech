using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Core.Interfaces;
using TechChallenge.Infrastructure.Repository;

namespace TechChallenge.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("Contacts"),
                    sqlServerOptions =>
                    {
                        sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(3), errorNumbersToAdd: []);
                    });
            });

            services.AddScoped<IContactRepository, ContactRepository>();

            return services;
        }
    }
}
