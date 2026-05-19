using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Persistence port for <see cref="LimitOrder"/> aggregates.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Returns the order with the given id, or <c>null</c> if not found.
    /// </summary>
    Task<LimitOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all orders regardless of status.
    /// </summary>
    Task<IReadOnlyList<LimitOrder>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns open (unfilled, uncancelled) orders, optionally filtered by currency pair.
    /// </summary>
    Task<IReadOnlyList<LimitOrder>> ListOpenAsync(CurrencyPair? pair = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order to the store.
    /// </summary>
    Task AddAsync(LimitOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing order (status, filled qty, price, etc.).
    /// </summary>
    Task UpdateAsync(LimitOrder order, CancellationToken cancellationToken = default);
}
