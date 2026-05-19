using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IGetTradesQuery
{
    Task<IReadOnlyList<Trade>> HandleAsync(int? limit, CancellationToken cancellationToken = default);
}
