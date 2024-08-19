using MassTransit;
using Microsoft.Extensions.Options;
using TechChallenge.API.Common.Configuration;

namespace TechChallenge.API.Common.Services
{
    public interface IRabbitMqService
    {
        Task<ISendEndpoint> GetEndpoint();
    }

    public class RabbitMqService(IOptions<RabbitMqConfiguration> options, IBus bus) : IRabbitMqService
    {
        private readonly RabbitMqConfiguration _configuration = options.Value;
        private readonly IBus _bus = bus;

        public async Task<ISendEndpoint> GetEndpoint()
        {
            var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{_configuration.QueueName}"));

            return endpoint;
        }
    }
}
