
using mydev.MCPSample.Api.Registrations;

namespace mydev.MCPSample.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
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

            app.UseAuthorization();
            
            app.MapGet("/healthz", (HttpContext httpContext) =>
            {
                return "Healthy";
            })
            .WithName("GetHealth")
            .WithOpenApi();
            app.RegisterEndpoints();
            app.MapMcp("/tasks-mcp");
            await app.RunAsync();
        }
    }
}
