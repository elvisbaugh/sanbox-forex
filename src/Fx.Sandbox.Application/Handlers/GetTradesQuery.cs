using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

public sealed class GetTradesQuery : IGetTradesQuery
{
    private readonly ITradeRepository _trades;
    public GetTradesQuery(ITradeRepository trades) => _trades = trades;

    public Task<IReadOnlyList<Trade>> HandleAsync(int? limit, CancellationToken cancellationToken = default) =>
        _trades.ListAsync(limit, cancellationToken);
}
