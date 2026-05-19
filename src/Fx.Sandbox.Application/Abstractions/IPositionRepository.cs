using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Persistence port for <see cref="Position"/> aggregates (one per currency pair).
/// </summary>
public interface IPositionRepository
{
    /// <summary>
    /// Returns the position for the pair, creating a flat (zero-quantity) one if absent.
    /// </summary>
    Task<Position> GetOrCreateAsync(CurrencyPair pair, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every tracked position.
    /// </summary>
    Task<IReadOnlyList<Position>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates the position keyed by its currency pair.
    /// </summary>
    Task UpsertAsync(Position position, CancellationToken cancellationToken = default);
}
