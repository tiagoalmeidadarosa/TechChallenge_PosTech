using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechChallenge.API.Delete.Configuration;
using TechChallenge.Core.Interfaces;

namespace TechChallenge.API.Delete.Controllers
{
    [Route("api/contacts/[controller]")]
    [ApiController]
    public sealed class DeleteController(ILogger<DeleteController> logger, IOptions<RabbitMqConfiguration> options,
        IBus bus, IContactRepository contactRepository) : ControllerBase
    {
        private readonly ILogger<DeleteController> _logger = logger;
        private readonly RabbitMqConfiguration _configuration = options.Value;
        private readonly IBus _bus = bus;
        private readonly IContactRepository _contactRepository = contactRepository;

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{_configuration.QueueName}"));
            await endpoint.Send(contact);

            _logger.LogInformation("Delete - Contact sent to the queue");

            return Accepted();
        }
    }
}
