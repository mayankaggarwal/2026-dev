using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SKOllamaAgent
{
    public interface IAppRunner
    {
        Task RunAsync();
    }

    public class AppRunner : IAppRunner 
    {
        private readonly ILogger<AppRunner> _logger;
        private readonly IHttpClientFactory _httpFactory;

        public AppRunner(ILogger<AppRunner> logger, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _httpFactory = httpFactory;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("AppRunner is running...");
            //await ChatApplication();
            await FunctionCalling();

        }

        //https://laurentkempe.com/2025/03/02/building-local-ai-agents-semantic-kernel-agent-with-functions-in-csharp-using-ollama/
        private async Task FunctionCalling()
        {
            var builder = Kernel.CreateBuilder();
            // 👇🏼 Using llama3.2 with Ollama
            // Create or get a configured HttpClient with an increased timeout and pass it to the connector if supported.
            var httpClient = _httpFactory.CreateClient("Ollama");
            // Some connector overloads accept an HttpClient instance. If not, the named client still ensures a configured client is available for reuse.
            builder.AddOllamaChatCompletion("qwen3:4b", httpClient);

            var kernel = builder.Build();

            ChatCompletionAgent agent = new() // 👈🏼 Definition of the agent
            {
                Instructions =
                """
    Answer questions about different locations.
    For France, use the time format: HH:MM.
    HH goes from 00 to 23 hours, MM goes from 00 to 59 minutes.
    """,
                Name = "Location Agent",
                Kernel = kernel,
                // 👇🏼 Allows the model to decide whether to call the function
                Arguments = new KernelArguments(new PromptExecutionSettings
                { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            };

            // 👇🏼 Define a time tool plugin
            var plugin =
                KernelPluginFactory.CreateFromFunctions(
                    "Time",
                    "Get the current time for a city",
                    [KernelFunctionFactory.CreateFromMethod(GetCurrentTime)]);
            agent.Kernel.Plugins.Add(plugin);

            ChatHistory chat =
            [
                new ChatMessageContent(AuthorRole.User, "What time is it in Illzach, France?")
            ];

            await foreach (var response in agent.InvokeAsync(chat))
            {
                chat.Add(response);
                Console.WriteLine(response.Message.Content);
            }
        }

        [Description("Get the current time for a city")]
        string GetCurrentTime(string city) =>
    $"It is {DateTime.Now.Hour}:{DateTime.Now.Minute} in {city}.";

        private async Task ChatApplication()
        {
            //https://laurentkempe.com/2025/03/01/building-local-ai-agents-semantic-kernel-and-ollama-in-csharp/
            var builder = Kernel.CreateBuilder();
            // 👇🏼 Using Phi-4 with Ollama
            builder.AddOllamaChatCompletion("qwen3:4b", new Uri("http://localhost:11434"));

            var kernel = builder.Build();

            ChatCompletionAgent agent = new() // 👈🏼 Definition of the agent
            {
                Instructions = "Answer questions about C# and .NET",
                Name = "C# Agent",
                Kernel = kernel
            };

            ChatHistory chat =
            [
                new ChatMessageContent(AuthorRole.User,
                           "What is the difference between a class and a record?")
            ];

            await foreach (var response in agent.InvokeAsync(chat))
            {
                chat.Add(response);
                Console.WriteLine(response.Message.Content);
            }
        }
    }
}
