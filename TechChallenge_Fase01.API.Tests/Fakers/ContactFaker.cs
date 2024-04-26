using Bogus;
using TechChallenge_Fase01.Core.Entities;

namespace TechChallenge_Fase01.API.Tests.Fakers
{
    public class ContactFaker : Faker<Contact> 
    {
        public ContactFaker(string locale) : base(locale)
        {
            RuleFor(x => x.Id, f => f.UniqueIndex);
            RuleFor(x => x.Name, f => f.Person.FullName);
            RuleFor(x => x.Phone, f => f.Random.ReplaceNumbers($"99#######"));
            RuleFor(x => x.Email, f => f.Person.Email);
            RuleFor(x => x.DDD, f => f.Random.Number(11, 99));
        }
    }
}