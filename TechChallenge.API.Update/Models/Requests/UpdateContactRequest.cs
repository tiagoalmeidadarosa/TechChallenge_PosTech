namespace TechChallenge.API.Update.Models.Requests
{
    public class UpdateContactRequest
    {
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int DDD { get; set; } = default!;
    }
}
