using AutoMapper;
using AzureUpskill.Models.Data.Mapping;
using AzureFunctions.Extensions.Swashbuckle;
using System.Reflection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AzureUpskill.Search.Mapping;
using AzureUpskill.Search.Services.Interfaces;
using AzureUpskill.Functions.Search;
using AzureUpskill.Functions.Search.Services;

[assembly: WebJobsStartup(typeof(AzureUpskill.Functions.Startup))]
namespace AzureUpskill.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddAutoMapper(new[] {
                typeof(CommonMapper).Assembly,
                typeof(CandidateIndexMapper).Assembly
            });
            RegisterDomainServices(builder);
        }

        private void RegisterDomainServices(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<ISearchIndexClientRegistry, CfgBasedSearchIndexClientRegistry>();
            builder.Services.AddScoped<ISearchServiceClientProvider, CfgBasedSearchIndexClientRegistry>();
        }
    }
}
