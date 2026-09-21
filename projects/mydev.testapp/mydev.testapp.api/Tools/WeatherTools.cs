using ModelContextProtocol.Server;
using mydev.testapp.api.ContextManager;
using System.ComponentModel;

namespace mydev.testapp.api.Tools
{
    [McpServerToolType]
    public class WeatherTools
    {
        private readonly IMcpContextService _contextService;
        // Dependency injection works natively here
        private readonly ILogger<WeatherTools> _logger;

        public WeatherTools(IMcpContextService contextService, ILogger<WeatherTools> logger)
        {
            _contextService = contextService;
            _logger = logger;
        }

        [McpServerTool(Name = "get-weather-forecast")]
        [Description("Retrieves the weather forecast for a specific city location." +
            "IMPORTANT:" +
            "Before calling this tool, if no customer context is currently selected," +
            "first call get_available_contexts and then call select_context using" +
            "the context selected by the user.")]
        public async Task<string> GetWeatherForecastAsync(
            [Description("The name of the city, e.g., 'London'")] string city,
            CancellationToken cancellationToken = default)
        {
            var context =
            await _contextService.GetRequiredContextAsync(
                cancellationToken);
            _logger.LogInformation("Executing authorized tool 'get-weather-forecast' for {City}", city);

            // Mock data logic layer
            var summary = city.ToLower() == "london" ? "Rainy and chilly" : "Sunny and clear";
            return $"The current weather forecast for {city} is: {summary}.";
        }
    }
}
