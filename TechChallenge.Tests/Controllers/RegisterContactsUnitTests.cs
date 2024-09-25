using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TechChallenge.API.Register.Configuration;
using TechChallenge.API.Register.Controllers;
using TechChallenge.API.Register.Models.Requests;
using TechChallenge.Consumer;
using TechChallenge.Consumer.Events;
using TechChallenge.Core.Entities;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;
using TechChallenge.Tests.Fakers;

namespace TechChallenge.Tests.Controllers
{
    public class RegisterContactsUnitTests : IDisposable
    {
        private readonly ContactFaker _contactFaker;
        private readonly IOptions<RabbitMqConfiguration> _options;
        private readonly ContactRepository _contactRepository;
        private readonly NullLogger<RegisterController> _logger;
        private readonly ITestHarness _harnessService;
        private readonly RegisterController _registerController;
        private readonly AppDbContext _dbContext;

        public RegisterContactsUnitTests()
        {
            _logger = NullLogger<RegisterController>.Instance;

            _dbContext = new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("Filename=RegisterContactsUnitTests.db").Options);
            _contactRepository = new ContactRepository(_dbContext);

            var rabbitMqConfigurationOptions = new RabbitMqConfiguration
            {
                QueueName = "contacts-register-test",
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
            };
            _options = Options.Create(rabbitMqConfigurationOptions);

            var provider = new ServiceCollection()
                .AddScoped(x => new RegisterContact(NullLogger<Worker>.Instance, _contactRepository))
                .AddMassTransitTestHarness(x =>
                {
                    x.AddDelayedMessageScheduler();
                    x.AddConsumer<RegisterContact>();

                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.UseDelayedMessageScheduler();

                        cfg.ReceiveEndpoint(_options.Value.QueueName, e =>
                        {
                            e.ConfigureConsumer<RegisterContact>(context);
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                })
                .BuildServiceProvider(true);
            _harnessService = provider.GetRequiredService<ITestHarness>();

            _registerController = new RegisterController(_logger, _options, _harnessService.Bus);
            _contactFaker = new ContactFaker("pt_BR");
        }

        [Fact]
        public async Task PostContact_WithValidFields_ShouldReturnContact()
        {
            // Arrange
            await _harnessService.Start();

            var contact = _contactFaker.Generate();
            var contactRequest = new RegisterContactRequest
            {
                DDD = contact.DDD,
                Email = contact.Email,
                Name = contact.Name,
                Phone = contact.Phone,
            };

            // Act
            var result = await _registerController.Post(contactRequest);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            Assert.True(await _harnessService.Sent.Any<Contact>());
            Assert.True(await _harnessService.Consumed.Any<Contact>());

            var consumer = _harnessService.GetConsumerHarness<RegisterContact>();
            Assert.True(await consumer.Consumed.Any<Contact>());

            var contactFromDb = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Name == contact.Name);
            Assert.NotNull(contactFromDb);
        }

        [Fact]
        public async Task PostContact_WithInvalidFields_ShouldReturnError()
        {
            // Act
            var result = await _registerController.Post(new()
            {
                DDD = new Random().Next(1, 10),
                Email = "aaa@",
                Name = "Error name",
                Phone = "+41534534322323",
            });

            // Assert
            Assert.IsType<ObjectResult>(result);

            var objectResult = (ObjectResult)result;
            var response = (ValidationProblemDetails)objectResult.Value!;

            Assert.True(response.Errors.Count == 3);
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