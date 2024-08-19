using FluentValidation;
using TechChallenge.API.Register.Models.Requests;

namespace TechChallenge.API.Register.Validators
{
    public sealed class ContactRequestValidator : AbstractValidator<RegisterContactRequest>
    {
        public ContactRequestValidator()
        {
            RuleFor(x => x.DDD)
                .InclusiveBetween(11, 99)
                .When(x => x is not null);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone is required.")
                .Matches(@"^\d{9}$")
                .WithMessage("Invalid phone number.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address.");
        }
    }
}
