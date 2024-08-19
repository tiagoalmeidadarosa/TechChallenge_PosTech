using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechChallenge.Consumer;
using TechChallenge.Consumer.Configuration;
using TechChallenge.Consumer.Events;
using TechChallenge.Core.Interfaces;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .Build();

builder.Services.AddHostedService<Worker>();

builder.Services.Configure<RabbitMqConfiguration>(a => builder.Configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(a));
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Contacts"),
        sqlServerOptions =>
        {
            sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(3), errorNumbersToAdd: []);
        });
});
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

        cfg.ReceiveEndpoint(rabbitMqConfiguration.Queues.Register, e =>
        {
            e.ConfigureConsumer<RegisterContact>(context);
        });

        //Todo: Register other queues here

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumer<RegisterContact>();

    //Todo: Register other consumers here
}));
builder.Services.AddScoped<IContactRepository, ContactRepository>();

var host = builder.Build();
await host.RunAsync();
