namespace mydev.testapp.api.ContextManager
{
    public enum McpClientType
    {
        Human = 1,
        ExternalCustomerAgent = 2,
        InternalEbbAgent = 3
    }
    public sealed record McpContext
    {
        public required string SessionId { get; init; }

        public required string UserId { get; init; }

        public required int CustomerId { get; init; }
        public required string ClientId { get; init; }
        public McpClientType ClientType { get; init; }

        public required string Role { get; init; }

        public string ContextId { get; init; } = Guid.NewGuid().ToString("N");

        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

        public DateTime LastAccessUtc { get; set; } = DateTime.UtcNow;
    }
}
