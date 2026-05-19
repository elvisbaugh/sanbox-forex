using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Handlers;

public sealed class GetCurrentRatesQuery : IGetCurrentRatesQuery
{
    private readonly IRateFeed _rates;
    public GetCurrentRatesQuery(IRateFeed rates) => _rates = rates;

    public IReadOnlyList<RateDto> Handle() =>
        _rates.Snapshot().Select(rate => new RateDto(rate.Pair, rate.Mid, rate.Timestamp)).ToList();
}
