using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechChallenge.API.Register.Configuration;
using TechChallenge.API.Register.Models.Requests;
using TechChallenge.API.Register.Validators;
using TechChallenge.Core.Entities;

namespace TechChallenge.API.Register.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class RegisterController(ILogger<RegisterController> logger, IOptions<RabbitMqConfiguration> options, IBus bus) : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger = logger;
        private readonly RabbitMqConfiguration _configuration = options.Value;
        private readonly IBus _bus = bus;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] RegisterContactRequest request)
        {
            ContactRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return ValidationProblem();
            }

            var contact = new Contact()
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                DDD = request.DDD,
            };

            var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{_configuration.QueueName}"));
            await endpoint.Send(contact);

            _logger.LogInformation("Contact sent to the queue");

            return Accepted();
        }
    }
}
