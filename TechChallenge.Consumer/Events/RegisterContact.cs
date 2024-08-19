using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Core.Entities;
using TechChallenge.Infrastructure;

namespace TechChallenge.Consumer.Events
{
    public class RegisterContact(ILogger<Worker> logger, IDbContextFactory<AppDbContext> dbContextFactory) : IConsumer<Contact>
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly AppDbContext _dbContext = dbContextFactory.CreateDbContext()
;
        public async Task Consume(ConsumeContext<Contact> context)
        {
            var contact = context.Message;

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Contact received from the queue: {ContactName}", contact.Name);
        }
    }
}
