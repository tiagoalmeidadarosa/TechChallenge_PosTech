using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechChallenge_Fase01.API.Validators;
using TechChallenge_Fase01.Core.Interfaces;
using TechChallenge_Fase01.Infrastructure;
using TechChallenge_Fase01.Infrastructure.Repository;

namespace TechChallenge_Fase01;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
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

        app.Run();
    }
}
