namespace TechChallenge.Consumer.Configuration
{
    public class RabbitMqConfiguration
    {
        public string RegisterQueueName { get; set; } = default!;
        public string UpdateQueueName { get; set; } = default!;
        public string DeleteQueueName { get; set; } = default!;
        public string HostName { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
