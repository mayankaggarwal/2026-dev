using ExtensionsAI.labs.Common;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OpenAI;
using System.Text.Json;

namespace ExtensionsAI.labs.Day3
{
    internal class MyProfileAssistantWithEvaluation : RunnerBase
    {
        private readonly ILogger<MyProfileAssistantWithEvaluation> _logger;
        private readonly IChatClient chatClient;
        private readonly IChatClient openAIChatClient;
        private const string name = "Mayank";
        private readonly string folderPath = "D:\\src\\2026-dev\\courses\\udemy-agentic-ai\\agents\\1_foundations\\me";
        private List<ChatMessage> chatHistory = new();
        private string systemPrompt =
                $"""
                You are acting as {name}. You are answering questions on {name}'s website,
                particularly questions related to {name}'s career, background, skills and experience. \
                Your responsibility is to represent {name} for interactions on the website as faithfully as possible. \
                You are given a summary of {name}'s background and LinkedIn profile which you can use to answer questions. \
                Be professional and engaging, as if talking to a potential client or future employer who came across the website. \
                If you don't know the answer, say so.
                """;

        private string evaluatorSystemPrompt = $"""
                You are an evaluator that decides whether a response to a question is acceptable. 
                You are provided with a conversation between a User and an Agent. Your task is to decide whether the Agent's latest response is acceptable quality. 
                The Agent is playing the role of {name} and is representing {name} on their website. 
                The Agent has been instructed to be professional and engaging, as if talking to a potential client or future employer who came across the website. 
                The Agent has been provided with context on {name} in the form of their summary and LinkedIn details. Here's the information:
                """;

        public MyProfileAssistantWithEvaluation(AppConfigurations appConfigurations
            , ILogger<MyProfileAssistantWithEvaluation> logger)
            : base(appConfigurations)
        {
            _logger = logger;
            chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2:latest");
            openAIChatClient = new OpenAIClient(_appConfigurations.OpenAIKey).GetChatClient(_appConfigurations.ModelName).AsIChatClient();
        }

        public async override Task Run()
        {
            PdfReader pdfReader = new(Path.Combine(folderPath, "linkedin.pdf"));

            string linkedin = string.Empty;
            using (PdfDocument pdfDoc = new(pdfReader))
            {
                int numberOfPages = pdfDoc.GetNumberOfPages();
                for (int page = 1; page <= numberOfPages; page++)
                {
                    linkedin += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page)) + "\n";
                }
            }

            Console.WriteLine(linkedin);
            string summary = string.Empty;
            using (var reader = new StreamReader(Path.Combine(folderPath, "summary.txt")))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    summary += line + "\n";
                }
            }

            Console.WriteLine(summary);
            systemPrompt += $"\n\n## Summary:\n{summary}\n\n## LinkedIn Profile:\n{linkedin}\n\n";
            systemPrompt += $"With this context, please chat with the user, always staying in character as {name}.";
            evaluatorSystemPrompt += $"\n\n## Summary:\n{summary}\n\n## LinkedIn Profile:\n{linkedin}\n\n";
            evaluatorSystemPrompt += $"With this context, please evaluate the latest response, replying with whether the response is acceptable and your feedback.";

            chatHistory.Add(new ChatMessage(ChatRole.System, systemPrompt));

            string userInput = string.Empty;


            Console.WriteLine("You can now start chatting with the assistant. Ask any question related to Mayank's career, background, skills or experience. Enter with no text to end the chat");

            do
            {
                Console.WriteLine("\nYour prompt:");
                userInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Exiting due to invalid input.");
                    continue;
                }

                var response = await Chat(userInput);
                chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));

            } while (!string.IsNullOrWhiteSpace(userInput));
        }

        private async Task<string> Chat(string userMessage)
        {
            List<ChatMessage> messages =
            [
                .. chatHistory,
                new ChatMessage(ChatRole.User, userMessage),
            ];

            var response = "";
            await foreach (ChatResponseUpdate item in
                chatClient.GetStreamingResponseAsync(messages))
            {
                Console.Write(item.Text);
                response += item.Text;
            }

            var evaluation = await Evaluate(response, userMessage);
            if(evaluation.isAcceptable)
            {
                Console.WriteLine("\nResponse is acceptable.");
            }
            else
            {
                Console.WriteLine("\nResponse is not acceptable. Feedback:");
                Console.WriteLine(evaluation.feedBack);
                response = await Rerun(response,userMessage,evaluation.feedBack);
            }

            return response;
        }

        private async Task<Evaluation> Evaluate(string reply, string userMessage)
        {
            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.System, evaluatorSystemPrompt),
                new ChatMessage(ChatRole.User, GetEvaluatorUserPrompt(reply,userMessage)),
            ];

            var data = await openAIChatClient.GetResponseAsync(messages, new ChatOptions() { ResponseFormat = ChatResponseFormat.ForJsonSchema<Evaluation>() });

            return JsonSerializer.Deserialize<Evaluation>(data.Text);

        }

        private async Task<string> Rerun(string reply, string userMessage, string feedback)
        {
            string updated_system_prompt = systemPrompt + "\n\n## Previous answer rejected\nYou just tried to reply, but the quality control rejected your reply\n";
            updated_system_prompt += $"## Your attempted answer:\n{reply}\n\n";
            updated_system_prompt += $"## Reason for rejection:\n{feedback}\n\n";
            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.System, updated_system_prompt),
                .. chatHistory,
                new ChatMessage(ChatRole.User, userMessage),
            ];

            var response = "";
            await foreach (ChatResponseUpdate item in
                chatClient.GetStreamingResponseAsync(messages))
            {
                Console.Write(item.Text);
                response += item.Text;
            }

            return response;
        }

        private string GetEvaluatorUserPrompt(string reply, string message)
        {
            string conversation = "";
            foreach (var item in chatHistory)
            {
                conversation += $"{item.Role}: {item.Text}\n";
            }
            string user_prompt = $"Here's the conversation between the User and the Agent: \n\n{conversation}\n\n";
            user_prompt += $"Here's the latest message from the User: \n\n{message}\n\n";
            user_prompt += $"Here's the latest response from the Agent: \n\n{reply}\n\n";
            user_prompt += "Please evaluate the response, replying with whether it is acceptable and your feedback.";
            return user_prompt;
        }
    }

    public class Evaluation
    {
        public bool isAcceptable { get; set;  }
        public string feedBack { get; set; }
    }


}
