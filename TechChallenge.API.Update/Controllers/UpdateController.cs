using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechChallenge.API.Update.Configuration;
using TechChallenge.API.Update.Models.Requests;
using TechChallenge.API.Update.Validators;
using TechChallenge.Core.Interfaces;

namespace TechChallenge.API.Update.Controllers
{
    [Route("api/contacts/[controller]")]
    [ApiController]
    public sealed class UpdateController(ILogger<UpdateController> logger, IOptions<RabbitMqConfiguration> options,
        IBus bus, IContactRepository contactRepository) : ControllerBase
    {
        private readonly ILogger<UpdateController> _logger = logger;
        private readonly RabbitMqConfiguration _configuration = options.Value;
        private readonly IBus _bus = bus;
        private readonly IContactRepository _contactRepository = contactRepository;

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromBody] UpdateContactRequest request)
        {
            ContactRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return ValidationProblem();
            }

            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            contact.Name = request.Name;
            contact.Phone = request.Phone;
            contact.Email = request.Email;
            contact.DDD = request.DDD;

            var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{_configuration.QueueName}"));
            await endpoint.Send(contact);

            _logger.LogInformation("Update - Contact sent to the queue");

            return Accepted();
        }
    }
}
