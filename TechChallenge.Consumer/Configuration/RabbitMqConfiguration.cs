namespace TechChallenge.Consumer.Configuration
{
    public class RabbitMqConfiguration
    {
        public Queues Queues { get; set; } = new();
        public string HostName { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class Queues
    {
        public string Register { get; set; } = default!;
        public string Update { get; set; } = default!;
        public string Delete { get; set; } = default!;
    }
}
