using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

public sealed class GetPositionsQuery : IGetPositionsQuery
{
    private readonly IPositionRepository _positions;
    private readonly IRateFeed _rates;

    public GetPositionsQuery(IPositionRepository positions, IRateFeed rates)
    {
        _positions = positions;
        _rates = rates;
    }

    public async Task<IReadOnlyList<PositionDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var all = await _positions.ListAsync(cancellationToken);
        return all.Where(position => !position.IsFlat).Select(position =>
        {
            var markPrice = _rates.CurrentRate(position.Pair).Mid;
            return new PositionDto(position.Pair, position.Quantity, position.AveragePrice, markPrice, PnLCalculatorService.Unrealised(position, markPrice), position.RealisedPnL);
        }).ToList();
    }
}
