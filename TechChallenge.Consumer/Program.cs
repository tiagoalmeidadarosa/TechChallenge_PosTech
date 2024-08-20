using MassTransit;
using Microsoft.Extensions.Options;
using TechChallenge.Consumer;
using TechChallenge.Consumer.Configuration;
using TechChallenge.Consumer.Events;
using TechChallenge.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .Build();

builder.Services.AddHostedService<Worker>();

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

        cfg.ReceiveEndpoint(rabbitMqConfiguration.Queues.Register, e =>
        {
            e.ConfigureConsumer<RegisterContact>(context);
        });

        cfg.ReceiveEndpoint(rabbitMqConfiguration.Queues.Update, e =>
        {
            e.ConfigureConsumer<UpdateContact>(context);
        });

        cfg.ReceiveEndpoint(rabbitMqConfiguration.Queues.Delete, e =>
        {
            e.ConfigureConsumer<DeleteContact>(context);
        });

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumer<RegisterContact>();
    x.AddConsumer<UpdateContact>();
    x.AddConsumer<DeleteContact>();
}));

var host = builder.Build();
await host.RunAsync();
