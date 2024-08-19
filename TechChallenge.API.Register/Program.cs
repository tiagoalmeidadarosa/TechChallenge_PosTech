using MassTransit;
using Microsoft.Extensions.Options;
using TechChallenge.API.Common;
using TechChallenge.API.Common.Configuration;

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
builder.Services.AddRabbitMqService(builder.Configuration);
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
