using Microsoft.Extensions.Caching.Memory;

namespace mydev.testapp.api.ContextManager
{
    public interface IMcpContextStore
    {
        Task<McpContext?> GetAsync(string sessionId, CancellationToken cancellationToken);

        Task SetAsync(string sessionId, McpContext context, TimeSpan expiration, CancellationToken cancellationToken);

        Task RemoveAsync(string sessionId, CancellationToken cancellationToken);
    }
    public sealed class MemoryMcpContextStore : IMcpContextStore
    {
        private readonly IMemoryCache _cache;

        public MemoryMcpContextStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<McpContext?> GetAsync(string sessionId, CancellationToken cancellationToken)
        {
            _cache.TryGetValue(GetKey(sessionId), out McpContext? context);

            return Task.FromResult(context);
        }

        public Task SetAsync(string sessionId,McpContext context, TimeSpan expiration, CancellationToken cancellationToken)
        {
            _cache.Set(GetKey(sessionId), context, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration });

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string sessionId, CancellationToken cancellationToken)
        {
            _cache.Remove(GetKey(sessionId));

            return Task.CompletedTask;
        }

        private static string GetKey(string sessionId) => $"mcp-context:{sessionId}";
    }
}
