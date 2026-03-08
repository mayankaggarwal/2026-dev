using mydev.MCPSample.Api.TaskManagement;

namespace mydev.MCPSample.Api.Registrations
{
    public static class McpRegistration
    {
        public static void RegisterMcpServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddMcpServer(o =>
            {
                o.ServerInfo = new()
                {
                    Name = "mysamplemcp",
                    Description = "My Sample API with MCP capabilities",
                    Version = "1.0.0"
                };
            })
            .WithHttpTransport()
            .WithPromptsFromAssembly()
            .WithToolsFromAssembly()
            .WithResources<ChartResources>();
        }
    }
}
