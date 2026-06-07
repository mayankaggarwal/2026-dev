using ExtensionsAI.labs.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
namespace ExtensionsAI.labs.Day1
{
    //https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/chat-local-model
    internal class OllamaExample : RunnerBase
    {
        private readonly ILogger<OllamaExample> _logger;
        public OllamaExample(AppConfigurations appConfigurations
            , ILogger<OllamaExample> logger)
            : base(appConfigurations)
        {
            _logger = logger;
        }

        public async override Task Run()
        {
            IChatClient chatClient =  new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2:latest");

            // Start the conversation with context for the AI model
            List<ChatMessage> chatHistory = new();

            while (true)
            {
                // Get user prompt and add to chat history
                Console.WriteLine("Your prompt:");
                var userPrompt = Console.ReadLine();
                chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

                // Stream the AI response and add to chat history
                Console.WriteLine("AI Response:");
                var response = "";
                await foreach (ChatResponseUpdate item in
                    chatClient.GetStreamingResponseAsync(chatHistory))
                {
                    Console.Write(item.Text);
                    response += item.Text;
                }
                chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
                Console.WriteLine();
            }
        }
    }
}
