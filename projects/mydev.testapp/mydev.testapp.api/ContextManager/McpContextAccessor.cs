using Microsoft.Extensions.Caching.Memory;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace mydev.testapp.api.ContextManager
{
    public interface IMcpContextAccessor
    {
        Task<McpContext?> GetAsync(CancellationToken cancellationToken = default);
        Task<McpContext> GetRequiredAsync(CancellationToken cancellationToken = default);

        Task SetAsync(McpContext context, CancellationToken cancellationToken = default);

        Task ClearAsync(CancellationToken cancellationToken = default);
    }
    public sealed class McpContextAccessor : IMcpContextAccessor
    {
        private const string SessionHeader = "Mcp-Session-Id";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMcpContextStore _contextStore;

        public McpContextAccessor(IHttpContextAccessor httpContextAccessor, IMcpContextStore contextStore)
        {
            _httpContextAccessor = httpContextAccessor;
            _contextStore = contextStore;
        }

        public async Task<McpContext?> GetAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = GetHttpContext();

            var principal = httpContext.User;

            if (principal.Identity?.IsAuthenticated != true)
                return null;

            var clientType = GetClientType(principal);

            /*
             * External customer agent and internal EBB agent
             * don't require MCP session-based context.
             */
            if (clientType != McpClientType.Human)
            {
                return await CreateMachineContextAsync(
                    principal,
                    clientType,
                    cancellationToken);
            }

            /*
             * Human client:
             * Customer/Role context is maintained against
             * the MCP session.
             */
            var sessionId = GetSessionId(httpContext);

            if (string.IsNullOrWhiteSpace(sessionId))
                return null;

            var context = await _contextStore.GetAsync(
                sessionId,
                cancellationToken);

            if (context == null)
                return null;

            /*
             * Never allow a cached context belonging to another
             * authenticated user to be reused.
             */
            var currentUserId = GetUserId(principal);

            if (!string.Equals(context.UserId, currentUserId, StringComparison.Ordinal))
            {
                await _contextStore.RemoveAsync(sessionId, cancellationToken);

                return null;
            }

            context.LastAccessUtc = DateTime.UtcNow;

            return context;
        }

        public async Task<McpContext> GetRequiredAsync(CancellationToken cancellationToken = default)
        {
            var context = await GetAsync(cancellationToken);

            if (context == null)
            {
                throw new InvalidOperationException("No customer context has been selected for the current MCP request.");
            }

            return context;
        }

        public async Task SetAsync(McpContext context, CancellationToken cancellationToken = default)
        {
            var httpContext = GetHttpContext();

            var principal = httpContext.User;

            if (principal.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException("The current MCP request is not authenticated.");

            /*
             * Only human clients use server-side session context.
             */
            if (context.ClientType != McpClientType.Human) throw new InvalidOperationException("Explicit context selection is only supported for human MCP clients.");

            var currentUserId = GetUserId(principal);

            if (!string.Equals(context.UserId, currentUserId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("The selected context does not belong to the authenticated user.");
            }

            var sessionId = GetSessionId(httpContext);

            if (string.IsNullOrWhiteSpace(sessionId)) throw new InvalidOperationException("MCP session ID is not available.");

            context.LastAccessUtc = DateTime.UtcNow;

            await _contextStore.SetAsync(sessionId, context, TimeSpan.FromHours(8), cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = GetHttpContext();

            var sessionId = GetSessionId(httpContext);

            if (string.IsNullOrWhiteSpace(sessionId))
                return;

            await _contextStore.RemoveAsync(sessionId, cancellationToken);
        }

        private async Task<McpContext?> CreateMachineContextAsync(ClaimsPrincipal principal, McpClientType clientType, CancellationToken cancellationToken)
        {
            var clientId = GetClientId(principal);

            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            /*
             * These claims are populated by the authentication/token
             * configuration for machine clients.
             *
             * ExternalCustomerAgent:
             *     ebb_customer_id
             *
             * InternalEbbAgent:
             *     ebb_customer_id
             *
             * In production these values should come from a trusted
             * server-side mapping rather than an arbitrary request value.
             */
            var customerIdValue = principal.FindFirst("ebb_customer_id")?.Value;

            if (!int.TryParse(customerIdValue, out var customerId))
                return null;

            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? principal.FindFirst("role")?.Value ?? "Service";

            return new McpContext
            {
                SessionId = null,
                UserId = null,
                ClientId = clientId,
                ClientType = clientType,
                CustomerId = customerId,
                Role = role,
                ContextId = $"machine-{clientId}-{customerId}",
                CreatedUtc = DateTime.UtcNow,
                LastAccessUtc = DateTime.UtcNow
            };
        }

        private static string? GetUserId(ClaimsPrincipal principal) => principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        private static string? GetClientId(ClaimsPrincipal principal) => principal.FindFirst("cid")?.Value ?? principal.FindFirst("client_id")?.Value;
        private static string GetSessionId(HttpContext httpContext) => httpContext.Request.Headers[SessionHeader].FirstOrDefault();
        private HttpContext GetHttpContext() => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context.");

        private static McpClientType GetClientType(ClaimsPrincipal principal)
        {
            var value = principal.FindFirst("ebb_client_type")?.Value;

            return value?.ToLowerInvariant() switch
            {
                "external-customer-agent" => McpClientType.ExternalCustomerAgent,

                "internal-ebb-agent" => McpClientType.InternalEbbAgent,

                _ => McpClientType.Human
            };
        }
    }
}
