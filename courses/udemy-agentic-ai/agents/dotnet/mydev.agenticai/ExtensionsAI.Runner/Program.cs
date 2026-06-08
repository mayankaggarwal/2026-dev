using ExtensionsAI.labs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace ExtensionsAI.Runner
{
    //https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/prompt-model?pivots=openai
    //https://github.com/openai/openai-dotnet
    //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-10.0&viewFallbackFrom=aspnetcore-3.0
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddUserSecrets<Program>();
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange:true);
                })
                .ConfigureServices((context,services) =>
                {
                    services.AddSingleton<IKeyedServiceProvider>(sp => (IKeyedServiceProvider)sp);
                    services.RegisterLabServices(context.Configuration);
                })
                .Build();

            var app = host.Services.GetRequiredService<ILabRunner>();
            await app.Run(5);
        }
    }
}
