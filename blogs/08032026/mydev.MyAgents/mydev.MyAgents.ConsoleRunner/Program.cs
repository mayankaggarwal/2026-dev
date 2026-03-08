using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SKOllamaAgent;

namespace mydev.MyAgents.ConsoleRunner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Register an HttpClient for the Ollama daemon with an increased timeout.
            // This uses the IHttpClientFactory and a named client so it can be reused.
            builder.Services.AddHttpClient("Ollama", c =>
            {
                c.BaseAddress = new Uri("http://localhost:11434");
                c.Timeout = TimeSpan.FromMinutes(10);
            });

            builder.Services.AddTransient<IAppRunner, AppRunner>();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            var app = builder.Build();

            var runner = app.Services.GetRequiredService<IAppRunner>();
            await runner.RunAsync();
        }
    }
}
