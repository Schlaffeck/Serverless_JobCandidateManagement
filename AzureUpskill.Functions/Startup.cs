using AutoMapper;
using AzureUpskill.Models.CreateCandidate.Mapping;
using AzureUpskill.Models.CreateCategory.Mapping;
using AzureUpskill.Models.Data.Mapping;
using AzureUpskill.Models.UpdateCandidate.Mapping;
using AzureUpskill.Models.UpdateCategory.Mapping;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureUpskill.Functions.Startup))]
namespace AzureUpskill.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAutoMapper(
                typeof(CommonMapper),
                typeof(UpdateCategoryInputMapper),
                typeof(CreateCategoryInputMapper),
                typeof(UpdateCandidateInputMapper),
                typeof(CreateCandidateInputMapper));
        }
    }
}
