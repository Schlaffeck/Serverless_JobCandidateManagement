using AutoMapper;
using AzureUpskill.Models.CreateCandidate.Mapping;
using AzureUpskill.Models.CreateCategory.Mapping;
using AzureUpskill.Models.Data.Mapping;
using AzureUpskill.Models.UpdateCandidate.Mapping;
using AzureUpskill.Models.UpdateCategory.Mapping;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;

[assembly: WebJobsStartup(typeof(AzureUpskill.Functions.Startup))]
namespace AzureUpskill.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddAutoMapper(typeof(CommonMapper).Assembly);
        }
    }
}
