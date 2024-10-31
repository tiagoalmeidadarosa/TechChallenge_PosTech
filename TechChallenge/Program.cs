using FluentValidation;
using Prometheus;
using TechChallenge.API.Models.Requests;
using TechChallenge.Infrastructure;

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
        builder.Services.AddValidatorsFromAssemblyContaining<FilteredContactsRequest>();

        builder.Services.AddInfrastructure(builder.Configuration);

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
