using System.Collections.Concurrent;
using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Infrastructure.Repositories;

public sealed class InMemoryPositionRepository : IPositionRepository
{
    private readonly ConcurrentDictionary<CurrencyPair, Position> _positions = new();

    public Task<Position> GetOrCreateAsync(CurrencyPair pair, CancellationToken cancellationToken = default)
        => Task.FromResult(_positions.GetOrAdd(pair, key => new Position(key)));

    public Task<IReadOnlyList<Position>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Position>>(_positions.Values.ToList());

    public Task UpsertAsync(Position position, CancellationToken cancellationToken = default)
    {
        _positions[position.Pair] = position;
        return Task.CompletedTask;
    }
}
