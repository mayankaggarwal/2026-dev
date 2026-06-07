using ExtensionsAI.labs.Common;
using Microsoft.Extensions.Logging;
using OpenAI;
namespace ExtensionsAI.labs.Day1
{
    internal class BenefitsIdentifier : RunnerBase
    {
        private readonly ILogger<BenefitsIdentifier> _logger;
        public BenefitsIdentifier(AppConfigurations appConfigurations
            ,ILogger<BenefitsIdentifier> logger) 
            : base(appConfigurations)
        {
            _logger = logger;
        }

        public async override Task Run()
        {
            var chatClient = new OpenAIClient(_appConfigurations.OpenAIKey).GetChatClient(_appConfigurations.ModelName);
            string text = File.ReadAllText(Path.Combine("Day1","benefits.md"));
            string prompt = $"""
                        Summarize the the following text in 20 words or less:
                        {text}
                        """;

            // Submit the prompt and print out the response.
            try
            {
                var response = await chatClient.CompleteChatAsync(prompt);
                if (response?.Value?.Content.Any() ?? false)
                {
                    var answer = response.Value.Content[0].Text;
                    _logger.LogInformation(answer);

                }
                else
                {
                    _logger.LogInformation("No response received.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a response from the chat client.");
            }

        }


        //    IChatClient client = new OpenAIClient(key).GetChatClient(model).AsIChatClient();
        //    string text = File.ReadAllText("benefits.md");
        //    string prompt = $"""
        //            Summarize the the following text in 20 words or less:
        //            {text}
        //            """;

        //    // Submit the prompt and print out the response.
        //    ChatResponse response = await client.GetResponseAsync(prompt, new ChatOptions { MaxOutputTokens = 400 });
        //    Console.WriteLine(response);
    }
}
