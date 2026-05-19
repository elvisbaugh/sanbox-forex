using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Infrastructure.Repositories;

public sealed class InMemoryTradeRepository : ITradeRepository
{
    private readonly object _lock = new();
    private readonly List<Trade> _trades = new();

    public Task AddAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        lock (_lock) _trades.Add(trade);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Trade>> ListAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IEnumerable<Trade> query = _trades.OrderByDescending(trade => trade.ExecutedAt);
            if (limit is not null) query = query.Take(limit.Value);
            return Task.FromResult<IReadOnlyList<Trade>>(query.ToList());
        }
    }
}
