using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TechChallenge.API.Common.Configuration;

namespace TechChallenge.API.Common.Services
{
    public interface IRabbitMqService
    {
        IConnection CreateChannel();
    }

    public class RabbitMqService(IOptions<RabbitMqConfiguration> options) : IRabbitMqService
    {
        private readonly RabbitMqConfiguration _configuration = options.Value;

        public IConnection CreateChannel()
        {
            ConnectionFactory connection = new()
            {
                UserName = _configuration.Username,
                Password = _configuration.Password,
                HostName = _configuration.HostName,
                //DispatchConsumersAsync = true
            };
            var channel = connection.CreateConnection();

            return channel;
        }
    }
}
