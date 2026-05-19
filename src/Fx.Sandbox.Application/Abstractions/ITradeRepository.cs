using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Persistence port for executed <see cref="Trade"/> records (immutable fill history).
/// </summary>
public interface ITradeRepository
{
    /// <summary>
    /// Appends a newly executed trade to the history.
    /// </summary>
    Task AddAsync(Trade trade, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns trades ordered by most recent first, optionally capped to <paramref name="limit"/> entries.
    /// </summary>
    Task<IReadOnlyList<Trade>> ListAsync(int? limit = null, CancellationToken cancellationToken = default);
}
