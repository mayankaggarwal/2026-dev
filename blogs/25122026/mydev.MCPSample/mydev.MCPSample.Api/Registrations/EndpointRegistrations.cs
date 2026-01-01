using mydev.MCPSample.Api.TaskManagement;

namespace mydev.MCPSample.Api.Registrations
{
    public static class EndpointRegistrations
    {
        public static void RegisterEndpoints(this WebApplication app)
        {
            var api = app.MapGroup("/api");
            TaskEndpoints.Add(api);
        }
    }
}
