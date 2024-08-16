using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using TechChallenge.API.Controllers;
using TechChallenge.API.Models.Responses;
using TechChallenge.Infrastructure;
using TechChallenge.Tests.Fakers;
using TechChallenge.API.Models.Requests;
using TechChallenge.Infrastructure.Repository;

namespace TechChallenge.Tests.Controllers
{
    public class ContactsUnitTests : IDisposable
    {
        private readonly ContactsController _contactsController;
        private readonly ContactFaker _contactFaker;
        private readonly AppDbContext _dbContext;

        public ContactsUnitTests()
        {
            _dbContext = new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("Filename=ContactTests.db").Options);

            var contactRepository = new ContactRepository(_dbContext);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<ContactsController>.Instance;

            _contactsController = new ContactsController(contactRepository, logger, cache);
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

        [Fact]
        public async Task PostContact_WithValidFields_ShouldReturnContact()
        {
            // Arrange
            var contact = _contactFaker.Generate();
            var contactRequest = new ContactRequest
            {
                DDD = contact.DDD,
                Email = contact.Email,
                Name = contact.Name,
                Phone = contact.Phone,
            };

            // Act
            var result = await _contactsController.Post(contactRequest);

            // Assert
            Assert.IsType<CreatedResult>(result.Result);

            var createdResult = (CreatedResult)result.Result;
            var response = (ContactResponse)createdResult.Value!;

            var contactFromDb = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == response.Id);
            Assert.NotNull(contactFromDb);
        }

        [Fact]
        public async Task PostContact_WithInvalidFields_ShouldReturnError()
        {
            // Act
            var result = await _contactsController.Post(new()
            {
                DDD = new Random().Next(1, 10),
                Email = "aaa@",
                Name = "Error name",
                Phone = "+41534534322323",
            });

            // Assert
            Assert.IsType<ObjectResult>(result.Result);

            var objectResult = (ObjectResult)result.Result;
            var response = (ValidationProblemDetails)objectResult.Value!;

            Assert.True(response.Errors.Count == 3);
        }

        [Fact]
        public async Task PutContact_WithValidFields_ShouldReturnNoContent()
        {
            // Arrange
            var contact = _contactFaker.Generate();

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            var newEmail = "newemail@gmail.com";
            var contactRequest = new ContactRequest
            {
                DDD = contact.DDD,
                Email = newEmail,
                Name = contact.Name,
                Phone = contact.Phone,
            };

            // Act
            var result = await _contactsController.Put(contact.Id, contactRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var contactFromDb = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == contact.Id);
            Assert.NotNull(contactFromDb);
            Assert.True(contactFromDb.Email == newEmail);
        }

        [Fact]
        public async Task DeleteContact_WithValidFields_ShouldReturnNoContent()
        {
            // Arrange
            var contact = _contactFaker.Generate();

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _contactsController.Delete(contact.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

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