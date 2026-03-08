using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace mydev.MCPSample.Api.Auth
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public const string ApiKeyHeaderName = "X-API-KEY";
        public string Scheme => DefaultScheme;
        public string AuthenticationType => DefaultScheme;
        public string ValidApiKey { get; set; } = string.Empty;
    }

    public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.Fail("API Key was not provided."));
            }
            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(providedApiKey) && providedApiKey.Equals(Options.ValidApiKey))
            {
                var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "ApiKeyUser") };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, Options.Scheme);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Options.Scheme);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key provided."));
        }
    }

    public static class ApiKeyAuthenticationOptionsExtensions
    {
        public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services, string validApiKey)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options =>
            {
                options.ValidApiKey = validApiKey;
            });
            return services;
        }
    }
}
