using MassTransit;
using TechChallenge.Core.Entities;
using TechChallenge.Core.Interfaces;

namespace TechChallenge.Consumer.Events
{
    public class DeleteContact(ILogger<Worker> logger, IContactRepository contactRepository) : IConsumer<Contact>
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IContactRepository _contactRepository = contactRepository;

        public async Task Consume(ConsumeContext<Contact> context)
        {
            var contact = context.Message;

            await _contactRepository.DeleteAsync(contact.Id);

            _logger.LogInformation("Contact deleted: {ContactName}", contact.Name);
        }
    }
}
