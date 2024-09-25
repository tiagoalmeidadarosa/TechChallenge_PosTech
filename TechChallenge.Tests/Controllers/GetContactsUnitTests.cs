using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechChallenge.API.Controllers;
using TechChallenge.API.Models.Responses;
using TechChallenge.Infrastructure;
using TechChallenge.Infrastructure.Repository;
using TechChallenge.Tests.Fakers;

namespace TechChallenge.Tests.Controllers
{
    public class GetContactsUnitTests : IDisposable
    {
        private readonly ContactsController _contactsController;
        private readonly ContactFaker _contactFaker;
        private readonly AppDbContext _dbContext;

        public GetContactsUnitTests()
        {
            _dbContext = new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("Filename=ContactTests.db").Options);

            var contactRepository = new ContactRepository(_dbContext);

            _contactsController = new ContactsController(contactRepository);
            _contactFaker = new ContactFaker("pt_BR");
        }

        [Fact]
        public async Task GetAllContacts_ShouldReturnContacts()
        {
            // Arrange
            var contacts = _contactFaker.Generate(10);

            _dbContext.Contacts.AddRange(contacts);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _contactsController.GetAll(new());

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);

            var okObjectResult = (OkObjectResult)result.Result;
            var response = (IEnumerable<ContactResponse>)okObjectResult.Value!;

            Assert.NotNull(response);
            Assert.True(response.Count() == 10);
        }

        [Fact]
        public async Task GetAllContacts_FilteringByDDD_ShouldReturnAtLeastOneContact()
        {
            // Arrange
            var contacts = _contactFaker.Generate(10);
            contacts[0].DDD = 11;

            _dbContext.Contacts.AddRange(contacts);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _contactsController.GetAll(new() { DDD = 11 });

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);

            var okObjectResult = (OkObjectResult)result.Result;
            var response = (IEnumerable<ContactResponse>)okObjectResult.Value!;

            Assert.NotNull(response);
            Assert.True(response.Any());
        }

        [Fact]
        public async Task GetAllContacts_WithInvalidDDD_ShouldReturnError()
        {
            // Arrange
            var contacts = _contactFaker.Generate(10);

            _dbContext.Contacts.AddRange(contacts);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _contactsController.GetAll(new() { DDD = 09 });

            // Assert
            Assert.IsType<ObjectResult>(result.Result);

            var objectResult = (ObjectResult)result.Result;
            var response = (ValidationProblemDetails)objectResult.Value!;

            Assert.True(response.Errors.Count == 1);
        }

        [Fact]
        public async Task GetContact_ShouldReturnContact()
        {
            // Arrange
            var contact = _contactFaker.Generate();

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _contactsController.Get(contact.Id);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);

            var okObjectResult = (OkObjectResult)result.Result;
            var response = (ContactResponse)okObjectResult.Value!;

            Assert.NotNull(response);
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