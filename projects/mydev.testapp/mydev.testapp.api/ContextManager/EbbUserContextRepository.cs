namespace mydev.testapp.api.ContextManager
{
    public interface IEbbUserContextRepository
    {
        Task<IReadOnlyList<AvailableMcpContext>> GetAvailableContextsAsync(string userId, CancellationToken cancellationToken);
        Task<AvailableMcpContext?> GetContextForUserAsync(string userId, string contextId, CancellationToken cancellationToken);
        Task<int?>
        GetCustomerIdForClientAsync(string clientId, CancellationToken cancellationToken = default);
    }
    public sealed class EbbUserContextRepository : IEbbUserContextRepository
    {

        public Task<IReadOnlyList<AvailableMcpContext>> GetAvailableContextsAsync(string userId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AvailableMcpContext> contexts =
            [
                new AvailableMcpContext
            {
                ContextId = "mayankaggarwal1989@gmail.com",
                CustomerId = 1001,
                CustomerName = "Customer A",
                Role = "Admin"
            },
            new AvailableMcpContext
            {
                ContextId = "mayankaggarwal1989@gmail.com",
                CustomerId = 1001,
                CustomerName = "Customer A",
                Role = "Sales"
            },
            new AvailableMcpContext
            {
                ContextId = "ctx-customer-b-admin",
                CustomerId = 2001,
                CustomerName = "Customer B",
                Role = "Admin"
            }
            ];

            return Task.FromResult(contexts);
        }

        public async Task<AvailableMcpContext?> GetContextForUserAsync(string userId, string contextId, CancellationToken cancellationToken = default)
        {
            var contexts = await GetAvailableContextsAsync(userId, cancellationToken);

            return contexts.FirstOrDefault(x => string.Equals(x.ContextId, contextId, StringComparison.Ordinal));
        }

        public async Task<int?> GetCustomerIdForClientAsync(string clientId, CancellationToken cancellationToken = default)
        {
            int? customerId = clientId switch
            {
                "customer-a-agent" => 1001,
                "customer-b-agent" => 2001,

                /*
                 * Internal EBB agent.
                 * Customer is normally supplied by trusted EBB
                 * execution context rather than being fixed here.
                 */
                _ => null
            };

            return customerId;
        }
    }
}
