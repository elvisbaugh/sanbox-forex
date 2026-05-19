using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

public sealed class GetRateHistoryQuery : IGetRateHistoryQuery
{
    private readonly IRateHistory _history;
    public GetRateHistoryQuery(IRateHistory history) => _history = history;

    public IReadOnlyList<RateDto> Handle(CurrencyPair pair, int count) =>
        _history.Recent(pair, count).Select(rate => new RateDto(rate.Pair, rate.Mid, rate.Timestamp)).ToList();
}
