using FluentValidation;
using TechChallenge_Fase01.Models.Requests;

namespace TechChallenge_Fase01.Validators
{
    internal sealed class CreateContactRequestValidator : AbstractValidator<CreateContactRequest>
    {
        public CreateContactRequestValidator()
        {
            RuleFor(p => p.DDD)
                .InclusiveBetween(11, 99)
                .When(x => x is not null);
        }
    }
}
