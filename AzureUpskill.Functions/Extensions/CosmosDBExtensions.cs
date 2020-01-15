using System;
using System.Collections.Generic;
using System.Text;
using AzureUpskill.Functions.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Newtonsoft.Json;

// based on: https://www.tpeczek.com/2019/01/azure-functions-20-extensibility.html
[assembly: WebJobsStartup(typeof(CosmosDbExtensionsWebJobsStartup))]
namespace AzureUpskill.Functions.Extensions
{
    public class CosmosDbExtensionsWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<CosmosDbExtensionExtensionsConfigProvider>();
        }
    }


    [Extension("CosmosDbExtensions")]
    internal class CosmosDbExtensionExtensionsConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.AddOpenConverter<IReadOnlyList<Document>, IReadOnlyList<OpenType>>(typeof(GenericDocumentConverter<>));
        }
    }

    internal class GenericDocumentConverter<T> : IConverter<IReadOnlyList<Document>, IReadOnlyList<T>>
    {
        public IReadOnlyList<T> Convert(IReadOnlyList<Document> input)
        {
            List<T> output = new List<T>(input.Count);

            foreach (Document item in input)
            {
                output.Add(Convert(item));
            }

            return output.AsReadOnly();
        }

        private static T Convert(Document document)
        {
            return JsonConvert.DeserializeObject<T>(document.ToString());
        }
    }
}
