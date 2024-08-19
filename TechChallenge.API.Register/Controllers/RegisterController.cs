using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.API.Common.Models.Requests;
using TechChallenge.API.Common.Services;
using TechChallenge.API.Common.Validators;
using TechChallenge.Core.Entities;

namespace TechChallenge.API.Register.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class RegisterController(ILogger<RegisterController> logger, IRabbitMqService rabbitMqService) : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger = logger;
        private readonly IRabbitMqService _rabbitMqService = rabbitMqService;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] ContactRequest request)
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

            var endpoint = await _rabbitMqService.GetEndpoint();
            await endpoint.Send(contact);

            _logger.LogInformation("Contact sent to the queue");

            return Accepted();
        }
    }
}
