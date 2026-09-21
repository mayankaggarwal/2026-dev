
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Authentication;
using mydev.testapp.api.ContextManager;

namespace mydev.testapp.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();

            builder.Services.AddSingleton<IMcpContextStore, MemoryMcpContextStore>();

            builder.Services.AddScoped<IMcpContextAccessor, McpContextAccessor>();

            builder.Services.AddScoped<IMcpContextService, McpContextService>();

            builder.Services.AddScoped<IEbbUserContextRepository, EbbUserContextRepository>();

            var OktaIssuer = builder.Configuration["Authentication:Authority"];
            var McpResource = builder.Configuration["Authentication:Audience"];
            // Add services to the container.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = OktaIssuer;
                options.Audience = McpResource;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            })
           .AddMcp(options =>
                {
                    options.ResourceMetadata = new ProtectedResourceMetadata
                    {
                        Resource = "http://localhost:5042/mcp",

                        AuthorizationServers =
                        {
                            OktaIssuer
                        },

                        ScopesSupported =
                        {
                            "ebb:mcp"
                        },

                        BearerMethodsSupported =
                        {
                            "header"
                        },

                        ResourceName = "EBB MCP Server"
                    };
                }); ;
            builder.Services.AddAuthorization(options =>
            {
                // Define a policy mapping to a specific OAuth scope or permission
                options.AddPolicy("McpToolsScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                context.User.Claims.Any(c =>
                    c.Type == "http://schemas.microsoft.com/identity/claims/scope" &&
                    c.Value == "ebb:mcp"));

                });
            });


            builder.Services.AddMcpServer(options =>
            {
                //options.ServerInfo.Name = "Secure-Dotnet10-MCP-Server";
                //options.ServerInfo.Version = "1.0.0";
            })
            .WithHttpTransport(options =>
            {
                options.Stateless = false;
            })
            .AddAuthorizationFilters()// Enables streamable HTTP transport for remote clients
            .WithToolsFromAssembly();
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine("=== AUTHENTICATED USER CLAIMS ===");

                    foreach (var claim in context.User.Claims)
                    {
                        Console.WriteLine($"{claim.Type} = {claim.Value}");
                    }
                }

                await next();
            });
            app.UseAuthorization();

            // 5. Map the standard MCP endpoint and secure it with the authorization policy
            app.MapMcp("/mcp")
               .RequireAuthorization("McpToolsScope");


            app.MapControllers();

            app.Run();
        }
    }
}


//http://127.0.0.1:33418
//https://vscode.dev/redirect