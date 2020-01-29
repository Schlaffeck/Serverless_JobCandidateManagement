using AzureUpskill.Functions.Queries.GetCategories.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Functions.Queries.GetCategories.Validation
{
    public class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
    {
        public GetCategoriesQueryValidator()
        {
            RuleFor(q => q.Skip).GreaterThanOrEqualTo(0).When(q => q.Skip.HasValue);
            RuleFor(q => q.Top).GreaterThanOrEqualTo(1).When(q => q.Top.HasValue);
        }
    }
}
