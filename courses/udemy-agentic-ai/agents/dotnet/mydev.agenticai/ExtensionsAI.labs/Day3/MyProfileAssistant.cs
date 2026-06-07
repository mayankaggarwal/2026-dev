using ExtensionsAI.labs.Common;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OpenAI;
using static iText.Kernel.Pdf.Colorspace.PdfPattern;

namespace ExtensionsAI.labs.Day3
{
    internal class MyProfileAssistant : RunnerBase
    {
        private readonly ILogger<MyProfileAssistant> _logger;
        public MyProfileAssistant(AppConfigurations appConfigurations
            , ILogger<MyProfileAssistant> logger)
            : base(appConfigurations)
        {
            _logger = logger;
        }

        public async override Task Run()
        {
            IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2:latest");
            //IChatClient chatClient = new OpenAIClient(_appConfigurations.OpenAIKey).GetChatClient(_appConfigurations.ModelName).AsIChatClient();
            string folderPath = "D:\\src\\2026-dev\\courses\\udemy-agentic-ai\\agents\\1_foundations\\me";
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

            string name = "Mayank Aggarwal";

            string system_prompt =
                $"""
                You are acting as {name}. You are answering questions on {name}'s website,
                particularly questions related to {name}'s career, background, skills and experience. \
                Your responsibility is to represent {name} for interactions on the website as faithfully as possible. \
                You are given a summary of {name}'s background and LinkedIn profile which you can use to answer questions. \
                Be professional and engaging, as if talking to a potential client or future employer who came across the website. \
                If you don't know the answer, say so.
                """;




            system_prompt += $"\n\n## Summary:\n{summary}\n\n## LinkedIn Profile:\n{linkedin}\n\n";
            system_prompt += $"With this context, please chat with the user, always staying in character as {name}.";

            List<ChatMessage> chatHistory = new();
            chatHistory.Add(new ChatMessage(ChatRole.System, system_prompt));

            string userInput = string.Empty;


            Console.WriteLine("You can now start chatting with the assistant. Ask any question related to Mayank's career, background, skills or experience. Enter with no text to end the chat");

            do
            {
                Console.WriteLine("\nYour prompt:");
                userInput = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Exiting due to invalid input.");
                    continue;
                }

                var response = await Chat(userInput, system_prompt, chatHistory, chatClient);
                chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));

            } while (!string.IsNullOrWhiteSpace(userInput));
        }

        private async Task<string> Chat(string userMessage, string systemPrompt, List<ChatMessage> chatHistory, IChatClient chatClient)
        {
            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.System, systemPrompt),
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
    }
}
