using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IGetCurrentRatesQuery
{
    IReadOnlyList<RateDto> Handle();
}
