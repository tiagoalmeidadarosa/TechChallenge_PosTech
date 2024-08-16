using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using TechChallenge.API.Validators;
using TechChallenge.Core.Interfaces;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;

namespace TechChallenge.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .Build();

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddValidatorsFromAssemblyContaining<ContactRequestValidator>();
        builder.Services.AddMemoryCache();

        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("Contacts"),
                sqlServerOptions =>
                {
                    sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(3), errorNumbersToAdd: []);
                });
        });
        builder.Services.AddScoped<IContactRepository, ContactRepository>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.UseHttpClientMetrics();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseMetricServer();
        app.UseHttpMetrics();

        app.Run();
    }
}
