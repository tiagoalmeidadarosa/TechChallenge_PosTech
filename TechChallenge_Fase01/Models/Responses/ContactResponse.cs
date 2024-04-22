namespace TechChallenge_Fase01.API.Models.Responses
{
    public class ContactResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int DDD { get; set; } = default!;
    }
}
