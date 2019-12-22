using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureUpskill.Models;
using System.Net;
using AzureUpskill.Models.CreateCategory;
using static AzureUpskill.Helpers.LogMessageHelper;

namespace AzureUpskill.CategoriesFunctions
{
    public static class CategoriesFunctions
    {
        [FunctionName("Category")]
        public static async Task<IActionResult> CreateCategory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                ConnectionStringSetting = "CosmosDbConnection")] IAsyncCollector<Category> categories,
            ILogger log)
        {
            try
            {
                log.LogInfo("START");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                CreateCategoryInput data = JsonConvert.DeserializeObject<CreateCategoryInput>(requestBody);
                if (string.IsNullOrWhiteSpace(data?.Name))
                {
                    log.LogInfo($"No name for category provided");
                    return new BadRequestObjectResult("Input object in wrong format");
                }

                var newCategory = new Category
                {
                    Name = data.Name
                };

                await categories.AddAsync(newCategory);

                log.LogInfo($"Creating category with name: {data.Name}");

                return new AcceptedResult();
            }
            finally
            {
                log.LogInfo("STOP");
            }
        }
    }
}
