
using mydev.MCPSample.Api.Auth;
using mydev.MCPSample.Api.Registrations;
using mydev.MCPSample.Api.TaskManagement;

namespace mydev.MCPSample.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApiKeyAuthentication(builder.Configuration["McpServerApiKey"]!);
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<ChartResources>();
            // Add services to the container.
            builder.RegisterMcpServices();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapGet("/healthz", (HttpContext httpContext) =>
            {
                return "Healthy";
            })
            .WithName("GetHealth");
            app.UseAuthentication();
            app.UseAuthorization();
            app.RegisterEndpoints();
            app.MapMcp("/tasks-mcp").RequireAuthorization();
            await app.RunAsync();
        }
    }
}
