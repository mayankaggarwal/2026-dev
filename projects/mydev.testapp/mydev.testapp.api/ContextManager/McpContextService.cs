using System.Security.Claims;

namespace mydev.testapp.api.ContextManager
{
    public interface IMcpContextService
    {
        Task<IReadOnlyList<AvailableMcpContext>> GetAvailableContextsAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

        Task<McpContext> SelectContextAsync(ClaimsPrincipal user, string contextId, CancellationToken cancellationToken);

        Task<McpContext?> GetCurrentContextAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
        Task<McpContext> GetRequiredContextAsync(CancellationToken cancellationToken = default);

        Task ClearContextAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
    }
    public sealed class McpContextService : IMcpContextService
    {
        private readonly IEbbUserContextRepository _repository;
        private readonly IMcpContextAccessor _contextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public McpContextService(
            IEbbUserContextRepository repository,
            IMcpContextAccessor contextAccessor,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _contextAccessor = contextAccessor;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IReadOnlyList<AvailableMcpContext>> GetAvailableContextsAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var principal = GetAuthenticatedPrincipal();
            var clientType = GetClientType(principal);

            if (clientType != McpClientType.Human)
            {
                throw new InvalidOperationException(
                    "Customer context selection is not required for machine clients.");
            }

            var userId = GetUserId(principal);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException(
                    "Authenticated user does not contain a subject.");

            return await _repository.GetAvailableContextsAsync(userId, cancellationToken);
        }

        public async Task<McpContext> SelectContextAsync(ClaimsPrincipal user, string contextId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(contextId))
                throw new ArgumentException(
                    "Context ID is required.",
                    nameof(contextId));

            var principal = GetAuthenticatedPrincipal();

            var clientType = GetClientType(principal);

            if (clientType != McpClientType.Human)
            {
                throw new InvalidOperationException(
                    "Machine clients cannot explicitly select customer context.");
            }

            var userId = GetUserId(principal);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException(
                    "Authenticated user does not contain a subject.");

            var selectedContext =
             await _repository.GetContextForUserAsync(
                 userId,
                 contextId,
                 cancellationToken);

            if (selectedContext == null)
            {
                throw new UnauthorizedAccessException(
                    "The requested context is not available to the authenticated user.");
            }

            var clientId = GetClientId(principal);

            var context = new McpContext
            {
                SessionId = GetSessionId(),
                UserId = userId,
                ClientId = clientId,
                ClientType = McpClientType.Human,
                CustomerId = selectedContext.CustomerId,
                Role = selectedContext.Role,
                ContextId = selectedContext.ContextId,
                CreatedUtc = DateTime.UtcNow,
                LastAccessUtc = DateTime.UtcNow
            };

            await _contextAccessor.SetAsync(context, cancellationToken);

            return context;
        }

        public Task<McpContext?> GetCurrentContextAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            return _contextAccessor.GetAsync(cancellationToken);
        }

        public Task<McpContext> GetRequiredContextAsync(CancellationToken cancellationToken = default)
        {
            return _contextAccessor.GetRequiredAsync(cancellationToken);
        }


        public Task ClearContextAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            return _contextAccessor.ClearAsync(cancellationToken);
        }

        private ClaimsPrincipal GetAuthenticatedPrincipal()
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException(
                    "The current MCP request is not authenticated.");
            }

            return principal;
        }

        private string GetUserId(ClaimsPrincipal principal)
        {
            return principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("Authenticated user does not contain a subject.");
        }

        private string? GetClientId(ClaimsPrincipal principal)
        {
            return principal.FindFirst("cid")?.Value ?? principal.FindFirst("client_id")?.Value;
        }

        private string? GetSessionId()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["Mcp-Session-Id"].FirstOrDefault();
        }

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
