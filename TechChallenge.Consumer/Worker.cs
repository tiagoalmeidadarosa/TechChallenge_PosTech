using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TechChallenge.API.Common.Services;
using TechChallenge.Core.Entities;
using TechChallenge.Infrastructure;

namespace TechChallenge.Consumer
{
    public class Worker(ILogger<Worker> logger, IRabbitMqService rabbitMqService, IDbContextFactory<AppDbContext> dbContextFactory) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
        private readonly AppDbContext _dbContext = dbContextFactory.CreateDbContext();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var connection = _rabbitMqService.CreateChannel();
                using var model = connection.CreateModel();
                model.QueueDeclare(queue: "ContactsQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(model);
                consumer.Received += async (sender, eventArgs) =>
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var contact = JsonSerializer.Deserialize<Contact>(message);

                    if (contact is null)
                    {
                        _logger.LogError("Failed to deserialize contact from message: {Message}", message);
                        return;
                    }

                    _dbContext.Contacts.Add(contact);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Contact received from the queue: {ContactName}", contact.Name);

                    await Task.CompletedTask;
                };

                model.BasicConsume("ContactsQueue", true, consumer);

                await Task.CompletedTask;
            }
        }
    }
}
