using ExtensionsAI.labs.Common;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp.Models;
using OpenAI;
using System.Diagnostics;

namespace ExtensionsAI.labs.Day4
{
    internal class OpenAIAgentsTracer : RunnerBase
    {
        private readonly ILogger<OpenAIAgentsTracer> _logger;
        public OpenAIAgentsTracer(AppConfigurations appConfigurations
            , ILogger<OpenAIAgentsTracer> logger)
            : base(appConfigurations)
        {
            _logger = logger;
        }

        public override async Task Run()
        {
            var openAIClient = new OpenAIClient(_appConfigurations.OpenAIKey);
            //var chatClient = openAIClient.GetChatClient(_appConfigurations.ModelName);
            //AIAgent agent = chatClient.AsAIAgent();
            //using (Activity? activity = AgentTracer.StartActivity("Telling a joke"))
            //{
            //    var finalOutput = await agent.RunAsync("Tell a joke about autonomous AI Agents");
            //    Console.WriteLine(finalOutput);
            //}


            // 2. Wrap it in a standard Microsoft Extensions AI ChatClient
            IChatClient chatClient = openAIClient.GetChatClient("gpt-4o-mini").AsIChatClient();

            // 2. Define the Agent utilizing the Microsoft.Agents.AI.OpenAI namespace
            var agent = new ChatClientAgent(
                chatClient: chatClient,
                name: "Jokester",
                instructions: "You are a joke teller"
            );

            using (Activity? activity = AgentTracer.StartActivity("Telling a joke"))
            {
                // The framework equivalent of Runner.run()
                var response = await agent.RunAsync("Tell a joke about Autonomous AI Agents");

                Console.WriteLine(response.Text);
            }

        }

        private static readonly ActivitySource AgentTracer = new("mydev.MayankAggarwal.AgentWorkflows");
    }
}
