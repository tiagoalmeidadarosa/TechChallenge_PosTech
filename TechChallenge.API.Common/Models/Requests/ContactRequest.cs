namespace TechChallenge.API.Common.Models.Requests
{
    public class ContactRequest
    {
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int DDD { get; set; } = default!;
    }
}
