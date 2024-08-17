using FluentValidation;
using TechChallenge.API.Common.Models.Requests;

namespace TechChallenge.API.Common.Validators
{
    public sealed class FilteredContactsRequestValidator : AbstractValidator<FilteredContactsRequest>
    {
        public FilteredContactsRequestValidator()
        {
            RuleFor(p => p.DDD)
                .InclusiveBetween(11, 99)
                .When(x => x is not null);
        }
    }
}
