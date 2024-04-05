﻿namespace TechChallenge_Fase01.Models.Requests
{
    public class CreateContactRequest
    {
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int DDD { get; set; } = default!;
    }
}
