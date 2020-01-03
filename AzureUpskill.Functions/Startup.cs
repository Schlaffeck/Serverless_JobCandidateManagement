using AutoMapper;
using AzureUpskill.Models.CreateCandidate.Mapping;
using AzureUpskill.Models.UpdateCandidate.Mapping;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureUpskill.Functions.Startup))]
namespace AzureUpskill.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAutoMapper(
                typeof(UpdateCandidateInputMapper),
                typeof(CreateCandidateInputMapper));
        }
    }
}
