using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TechChallenge.API.Delete.Configuration;
using TechChallenge.API.Delete.Controllers;
using TechChallenge.Consumer;
using TechChallenge.Consumer.Events;
using TechChallenge.Core.Entities;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;
using TechChallenge.Tests.Fakers;

namespace TechChallenge.Tests.Controllers
{
    public class DeleteContactsUnitTests : IDisposable
    {
        private readonly ContactFaker _contactFaker;
        private readonly IOptions<RabbitMqConfiguration> _options;
        private readonly ContactRepository _contactRepository;
        private readonly NullLogger<DeleteController> _logger;
        private readonly ITestHarness _harnessService;
        private readonly DeleteController _deleteController;
        private readonly AppDbContext _dbContext;

        public DeleteContactsUnitTests()
        {
            _logger = NullLogger<DeleteController>.Instance;

            _dbContext = new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("Filename=DeleteContactsUnitTests.db").Options);
            _contactRepository = new ContactRepository(_dbContext);

            var rabbitMqConfigurationOptions = new RabbitMqConfiguration
            {
                QueueName = "contacts-delete-test",
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
            };
            _options = Options.Create(rabbitMqConfigurationOptions);

            var provider = new ServiceCollection()
                .AddScoped(x => new DeleteContact(NullLogger<Worker>.Instance, _contactRepository))
                .AddMassTransitTestHarness(x =>
                {
                    x.AddDelayedMessageScheduler();
                    x.AddConsumer<DeleteContact>();

                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.UseDelayedMessageScheduler();

                        cfg.ReceiveEndpoint(_options.Value.QueueName, e =>
                        {
                            e.ConfigureConsumer<DeleteContact>(context);
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                })
                .BuildServiceProvider(true);
            _harnessService = provider.GetRequiredService<ITestHarness>();

            _deleteController = new DeleteController(_logger, _options, _harnessService.Bus, _contactRepository);
            _contactFaker = new ContactFaker("pt_BR");
        }

        [Fact]
        public async Task DeleteContact_WithValidFields_ShouldReturnNoContent()
        {
            // Arrange
            await _harnessService.Start();

            var contact = _contactFaker.Generate();

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _deleteController.Delete(contact.Id);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.True(await _harnessService.Sent.Any<Contact>());
            Assert.True(await _harnessService.Consumed.Any<Contact>());

            var consumer = _harnessService.GetConsumerHarness<DeleteContact>();
            Assert.True(await consumer.Consumed.Any<Contact>());

            var contactFromDb = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == contact.Id);
            Assert.Null(contactFromDb);
        }

        // Dispose pattern
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _dbContext.Database.EnsureDeleted();
            }

            _disposed = true;
        }
    }
}