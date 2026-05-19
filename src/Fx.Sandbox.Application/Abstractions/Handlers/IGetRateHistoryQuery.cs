using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IGetRateHistoryQuery
{
    IReadOnlyList<RateDto> Handle(CurrencyPair pair, int count);
}
