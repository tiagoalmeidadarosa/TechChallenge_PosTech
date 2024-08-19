namespace TechChallenge.API.Common.Configuration
{
    public class RabbitMqConfiguration
    {
        public string QueueName { get; set; } = default!;
        public string HostName { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
