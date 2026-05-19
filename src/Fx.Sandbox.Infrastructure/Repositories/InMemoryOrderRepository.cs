using System.Collections.Concurrent;
using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Infrastructure.Repositories;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, LimitOrder> _orders = new();

    public Task<LimitOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_orders.TryGetValue(id, out var order) ? order : null);

    public Task<IReadOnlyList<LimitOrder>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<LimitOrder>>(_orders.Values.ToList());

    public Task<IReadOnlyList<LimitOrder>> ListOpenAsync(CurrencyPair? pair = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<LimitOrder> query = _orders.Values.Where(order => order.Status == OrderStatus.Open);

        if (pair is not null) query = query.Where(order => order.Pair == pair.Value);

        return Task.FromResult<IReadOnlyList<LimitOrder>>(query.ToList());
    }

    public Task AddAsync(LimitOrder order, CancellationToken cancellationToken = default)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LimitOrder order, CancellationToken cancellationToken = default)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
