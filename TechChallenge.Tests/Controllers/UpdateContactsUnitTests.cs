using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TechChallenge.API.Update.Configuration;
using TechChallenge.API.Update.Controllers;
using TechChallenge.API.Update.Models.Requests;
using TechChallenge.Consumer;
using TechChallenge.Consumer.Events;
using TechChallenge.Core.Entities;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;
using TechChallenge.Tests.Fakers;

namespace TechChallenge.Tests.Controllers
{
    public class UpdateContactsUnitTests : IDisposable
    {
        private readonly ContactFaker _contactFaker;
        private readonly IOptions<RabbitMqConfiguration> _options;
        private readonly ContactRepository _contactRepository;
        private readonly NullLogger<UpdateController> _logger;
        private readonly ITestHarness _harnessService;
        private readonly UpdateController _updateController;
        private readonly AppDbContext _dbContext;

        public UpdateContactsUnitTests()
        {
            _logger = NullLogger<UpdateController>.Instance;

            _dbContext = new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("Filename=UpdateContactsUnitTests.db").Options);
            _contactRepository = new ContactRepository(_dbContext);

            var rabbitMqConfigurationOptions = new RabbitMqConfiguration
            {
                QueueName = "contacts-update-test",
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
            };
            _options = Options.Create(rabbitMqConfigurationOptions);

            var provider = new ServiceCollection()
                .AddScoped(x => new UpdateContact(NullLogger<Worker>.Instance, _contactRepository))
                .AddMassTransitTestHarness(x =>
                {
                    x.AddDelayedMessageScheduler();
                    x.AddConsumer<UpdateContact>();

                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.UseDelayedMessageScheduler();

                        cfg.ReceiveEndpoint(_options.Value.QueueName, e =>
                        {
                            e.ConfigureConsumer<UpdateContact>(context);
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                })
                .BuildServiceProvider(true);
            _harnessService = provider.GetRequiredService<ITestHarness>();

            _updateController = new UpdateController(_logger, _options, _harnessService.Bus, _contactRepository);
            _contactFaker = new ContactFaker("pt_BR");
        }

        [Fact]
        public async Task PutContact_WithValidFields_ShouldReturnNoContent()
        {
            // Arrange
            await _harnessService.Start();

            var contact = _contactFaker.Generate();

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            var newEmail = "newemail@gmail.com";
            var contactRequest = new UpdateContactRequest
            {
                DDD = contact.DDD,
                Email = newEmail,
                Name = contact.Name,
                Phone = contact.Phone,
            };

            // Act
            var result = await _updateController.Put(contact.Id, contactRequest);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.True(await _harnessService.Sent.Any<Contact>());
            Assert.True(await _harnessService.Consumed.Any<Contact>());

            var consumer = _harnessService.GetConsumerHarness<UpdateContact>();
            Assert.True(await consumer.Consumed.Any<Contact>());

            var contactFromDb = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == contact.Id);
            Assert.NotNull(contactFromDb);
            Assert.True(contactFromDb.Email == newEmail);
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