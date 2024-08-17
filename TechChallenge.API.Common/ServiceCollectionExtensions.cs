using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.API.Common.Configuration;
using TechChallenge.API.Common.Services;

namespace TechChallenge.API.Common
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRabbitMqService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqConfiguration>(a => configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(a));
            services.AddSingleton<IRabbitMqService, RabbitMqService>();
        }
    }
}
