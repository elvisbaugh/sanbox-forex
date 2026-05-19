using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

public sealed class GetAccountSummaryQuery : IGetAccountSummaryQuery
{
    private readonly IAccountRepository _accounts;
    private readonly IPositionRepository _positions;
    private readonly IRateFeed _rates;

    public GetAccountSummaryQuery(IAccountRepository accounts, IPositionRepository positions, IRateFeed rates)
    {
        _accounts = accounts;
        _positions = positions;
        _rates = rates;
    }

    public async Task<AccountSummaryDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetAsync(cancellationToken);
        var positions = await _positions.ListAsync(cancellationToken);
        decimal unrealised = 0m;
        foreach (var position in positions)
        {
            if (!position.IsFlat)
            {
                var markPrice = _rates.CurrentRate(position.Pair).Mid;
                unrealised += PnLCalculatorService.Unrealised(position, markPrice);
            }
        }
        var nav = account.Cash + unrealised;
        return new AccountSummaryDto(account.Cash, account.RealisedPnL, unrealised, nav);
    }
}
