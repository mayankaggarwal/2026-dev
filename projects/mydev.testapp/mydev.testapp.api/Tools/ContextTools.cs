using ModelContextProtocol.Server;
using mydev.testapp.api.ContextManager;
using System.ComponentModel;
using System.Security.Claims;

namespace mydev.testapp.api.Tools
{
    [McpServerToolType]
    public sealed class ContextTools
    {
        private readonly IMcpContextService _contextService;

        public ContextTools(IMcpContextService contextService)
        {
            _contextService = contextService;
        }

        [McpServerTool]
        [Description(
            "Gets the customers and roles that the authenticated EBB user can access. " +
            "If multiple contexts are returned, ask the user which customer and role " +
            "they want to use before calling select_context.")]
        public async Task<IReadOnlyList<AvailableMcpContext>> GetAvailableContexts(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            return await _contextService.GetAvailableContextsAsync(user, cancellationToken);
        }

        [McpServerTool]
        [Description("Selects the customer and role context for the current MCP session. " +
            "The contextId must come from get_available_contexts. " +
            "Never guess or fabricate a contextId.")]
        public async Task<McpContextResponse> SelectContext(string contextId, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var context = await _contextService.SelectContextAsync(user, contextId, cancellationToken);

            return new McpContextResponse
            {
                ContextId = context.ContextId,
                CustomerId = context.CustomerId,
                Role = context.Role
            };
        }

        [McpServerTool]
        [Description("Returns the currently selected EBB customer and role context for this MCP session.")]
        public async Task<McpContextResponse?> GetCurrentContext(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var context = await _contextService.GetCurrentContextAsync(user, cancellationToken);

            if (context is null)
            {
                return null;
            }

            return new McpContextResponse
            {
                ContextId = context.ContextId,
                CustomerId = context.CustomerId,
                Role = context.Role
            };
        }

        [McpServerTool]
        [Description("Clears the currently selected EBB customer and role context.")]
        public async Task ClearContext(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            await _contextService.ClearContextAsync(user, cancellationToken);

            return;
        }
    }

    public sealed record McpContextResponse
    {
        public required string ContextId { get; init; }

        public required int CustomerId { get; init; }

        public required string Role { get; init; }
    }
}
