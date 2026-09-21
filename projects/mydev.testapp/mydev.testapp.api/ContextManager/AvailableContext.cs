using ModelContextProtocol.Protocol;

namespace mydev.testapp.api.ContextManager
{
    public sealed record AvailableMcpContext
    {
        public string ContextId { get; init; } = default!;

        public int CustomerId { get; init; }

        public string CustomerName { get; init; } = default!;

        public string Role { get; init; } = default!;
    }
}
