using Bogus;
using TechChallenge.Core.Entities;

namespace TechChallenge.Tests.Fakers
{
    public class ContactFaker : Faker<Contact>
    {
        public ContactFaker(string locale) : base(locale)
        {
            RuleFor(x => x.Name, f => f.Person.FullName);
            RuleFor(x => x.Phone, f => f.Random.ReplaceNumbers($"99#######"));
            RuleFor(x => x.Email, f => f.Person.Email);
            RuleFor(x => x.DDD, f => f.Random.Number(11, 99));
        }
    }
}