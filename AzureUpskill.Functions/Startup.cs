using AutoMapper;
using AzureUpskill.Models.Data.Mapping;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using AzureUpskill.Search.Mapping;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AzureUpskill.Functions.Services.Search;
using Microsoft.Extensions.Options;

[assembly: WebJobsStartup(typeof(AzureUpskill.Functions.Startup))]
namespace AzureUpskill.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // added to fix deserializing json collections in body
            //builder.Services.AddTransient<IConfigureOptions<MvcOptions>, MvcJsonMvcOptionsSetup>();
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddAutoMapper(new[] {
                typeof(CommonMapper).Assembly,
                typeof(CandidateIndexMapper).Assembly,
                typeof(Startup).Assembly
            });
            RegisterConfiguration(builder);
            RegisterDomainServices(builder);
        }

        private void RegisterConfiguration(IWebJobsBuilder webJobsBuilder)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = configBuilder.Build();
            webJobsBuilder.Services.AddSingleton(configuration);
        }

        private void RegisterDomainServices(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<ISearchIndexClientRegistry, CfgBasedSearchIndexClientRegistry>();
            builder.Services.AddScoped<ISearchServiceClientProvider, CfgBasedSearchIndexClientRegistry>();
        }
    }
}
