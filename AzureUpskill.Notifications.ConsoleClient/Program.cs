using CommandLine;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureUpskill.Notifications.ConsoleClient
{
    class Program
    {
        private const string ApiPathBase = "http://localhost:7071/api/subscriptions";
        static async Task Main(string[] args)
        {
            Options options = default;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                options = o;
            });

            var user = options.UserId;
            Console.WriteLine($"User: {user} listening for candidate changes");
            var url = $"{ApiPathBase}/initiate/{user}";
            var connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            connection.On<NewCandidateMessage>("onNewCandidateAvailable", (message) =>
            {
                Console.WriteLine($"General: New candidate '{message.CandidateId}' available");
            });

            connection.On<NewCandidateMessage>("onNewCandidateInCategory", (message) =>
            {
                Console.WriteLine($"Category '{message.CategoryId}': New candidate '{message.CandidateId}' available");
            });

            await connection.StartAsync();
            if (options.All)
            {
                await SubscribeToGeneralGroupAsync(user);
            }

            if (options.Categories.Any())
            {
                await SubscribeToCategoryGroup(user, options.Categories);
            }

            Console.ReadKey();
        }

        private static async Task SubscribeToGeneralGroupAsync(string user)
        {
            using (var client = new HttpClient())
            {
                var resp = await client.PostAsync($"{ApiPathBase}/onNewCandidateAvailable",
                    new StringContent(JsonConvert.SerializeObject(new { UserId = user })));
            }
        }

        private static async Task SubscribeToCategoryGroup(string user, IEnumerable<string> categories)
        {
            using (var client = new HttpClient())
            {
                var stringContent = JsonConvert.SerializeObject(new { UserId = user, CategoryIds = categories.ToArray() });
                var resp = await client.PostAsync($"{ApiPathBase}/onNewCandidateInCategory",
                    new StringContent(stringContent));
            }
        }

        public class Options
        {
            [Option('u', "user", Default = "Console_test_app")]
            public string UserId { get; set; }

            [Option('a', "all")]
            public bool All { get; set; }

            [Option('c', "categories", Default = new string[0])]
            public IEnumerable<string> Categories { get; set; }
        }
    }
}
