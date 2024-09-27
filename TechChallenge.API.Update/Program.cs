using MassTransit;
using Microsoft.Extensions.Options;
using Prometheus;
using TechChallenge.API.Update.Configuration;
using TechChallenge.Infrastructure;

namespace TechChallenge.API.Update;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .Build();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.Configure<RabbitMqConfiguration>(a => builder.Configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(a));
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddMassTransit((x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfiguration = context.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;

                cfg.Host(rabbitMqConfiguration.HostName, "/", h =>
                {
                    h.Username(rabbitMqConfiguration.Username);
                    h.Password(rabbitMqConfiguration.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        }));

        builder.Services.UseHttpClientMetrics();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseMetricServer();
        app.UseHttpMetrics();

        await app.RunAsync();
    }
}
