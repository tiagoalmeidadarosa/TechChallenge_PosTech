using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using TechChallenge_Fase01.API.Controllers;
using TechChallenge_Fase01.API.Models.Responses;
using TechChallenge_Fase01.Infrastructure;
using TechChallenge_Fase01.Infrastructure.Repository;
using TechChallenge_Fase01.Tests.Fakers;

namespace TechChallenge_Fase01.Tests.Controllers
{
    public class ContactsUnitTests
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
        public void GenerateContactFaker_ShouldNotBeNull()
        {
            var contact = _contactFaker.Generate();

            Assert.NotNull(contact);
        }

        [Fact]
        public async Task PostContact_WithValidFields_ShouldReturnContact()
        {
            var contact = _contactFaker.Generate();

            var result = await _contactsController.Post(new()
            {
                DDD = contact.DDD,
                Email = contact.Email,
                Name = contact.Name,
                Phone = contact.Phone,
            });

            Assert.IsType<CreatedResult>(result.Result);

            var createdResult = (CreatedResult)result.Result;
            var response = (ContactResponse)createdResult.Value!;

            var contactFromDb = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == response.Id);
            Assert.NotNull(contactFromDb);
        }

        [Fact]
        public async Task PostContact_WithInvalidFields_ShouldReturnError()
        {
            var result = await _contactsController.Post(new()
            {
                DDD = new Random().Next(1, 10),
                Email = "aaa@",
                Name = "Error name",
                Phone = "+41534534322323",
            });

            Assert.IsType<ObjectResult>(result.Result);

            var objectResult = (ObjectResult)result.Result;
            var response = (ValidationProblemDetails)objectResult.Value!;

            Assert.True(response.Errors.Count == 3);
        }
    }
}