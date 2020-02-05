using AzureUpskill.Functions.Queries.GetCandidates.Models;
using FluentValidation;

namespace AzureUpskill.Functions.Queries.GetCandidates.Validation
{
    public class GetCandidatesQueryValidator : AbstractValidator<GetCandidatesQuery>
    {
        public GetCandidatesQueryValidator()
        {
            RuleFor(q => q.Skip).GreaterThanOrEqualTo(0).When(q => q.Skip.HasValue);
            RuleFor(q => q.Top).GreaterThanOrEqualTo(1).When(q => q.Top.HasValue);
        }
    }
}
