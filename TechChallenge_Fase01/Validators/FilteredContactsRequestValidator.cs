using FluentValidation;
using TechChallenge_Fase01.Models.Requests;

namespace TechChallenge_Fase01.Validators
{
    internal sealed class FilteredContactsRequestValidator : AbstractValidator<FilteredContactsRequest>
    {
        public FilteredContactsRequestValidator()
        {
            RuleFor(p => p.DDD)
                .InclusiveBetween(11, 99)
                .When(x => x is not null);
        }
    }
}
