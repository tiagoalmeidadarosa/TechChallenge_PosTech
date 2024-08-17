using Microsoft.EntityFrameworkCore;
using TechChallenge.API.Common;
using TechChallenge.Consumer;
using TechChallenge.Core.Interfaces;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;

var builder = Host.CreateApplicationBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .Build();

builder.Services.AddHostedService<Worker>();
builder.Services.AddRabbitMqService(configuration);

builder.Services.AddDbContextFactory<AppDbContext>(opt =>
{
    opt.UseSqlServer(configuration.GetConnectionString("Contacts"),
        sqlServerOptions =>
        {
            sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(3), errorNumbersToAdd: []);
        });
});
builder.Services.AddSingleton<IContactRepository, ContactRepository>();

var host = builder.Build();
host.Run();
