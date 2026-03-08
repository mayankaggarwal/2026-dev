using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace mydev.MCPSample.Api.TaskManagement
{
    [McpServerPromptType]
    public sealed class TaskPrompts
    {
        [McpServerPrompt(Name = "weather"),Description("Prompt for weather queries")]
        public string CallWeather()
        {
            return """
                If the user asks about weather, call `get_weather`
                with the provided city name.
                """;
        }
    }

    [McpServerToolType]
    public sealed class WeatherQueries
    {
        [McpServerTool(Name = "get_weather"), Description("Get weather information for a city")]
        public static string GetWeather(string city)
        {
            // In a real implementation, this would call a weather API.
            return $"The current weather in {city} is Sunny, 25°C.";
        }
    }
}
