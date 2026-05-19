using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

/// <summary>
/// Matches all resting limit orders for a pair against the latest rate
/// and applies any fills to positions / cash / trade log.
/// Caller is responsible for serialising calls across pairs if needed.
/// </summary>
public sealed class OnRateTickHandler : IOnRateTickHandler
{
    private readonly IOrderRepository _orders;
    private readonly IPositionRepository _positions;
    private readonly IAccountRepository _accounts;
    private readonly ITradeRepository _trades;
    private readonly IClock _clock;

    public OnRateTickHandler(
        IOrderRepository orders,
        IPositionRepository positions,
        IAccountRepository accounts,
        ITradeRepository trades,
        IClock clock)
    {
        _orders = orders;
        _positions = positions;
        _accounts = accounts;
        _trades = trades;
        _clock = clock;
    }

    public async Task HandleAsync(Rate rate, CancellationToken cancellationToken = default)
    {
        var openOrders = await _orders.ListOpenAsync(rate.Pair, cancellationToken);
        if (openOrders.Count == 0) return;

        // Stable order: oldest first.
        foreach (var order in openOrders.OrderBy(o => o.CreatedAt))
        {
            if (!LimitOrderMatcherService.ShouldFill(order, rate.Mid)) continue;

            var fillPrice = rate.Mid;
            var now = _clock.UtcNow;
            order.Fill(fillPrice, now);
            await _orders.UpdateAsync(order, cancellationToken);

            var position = await _positions.GetOrCreateAsync(order.Pair, cancellationToken);
            var realisedDelta = PositionCalculatorService.ApplyFill(position, order.Side, order.Quantity, fillPrice);
            await _positions.UpsertAsync(position, cancellationToken);

            if (realisedDelta != 0m)
            {
                var account = await _accounts.GetAsync(cancellationToken);
                account.Settle(realisedDelta);
                await _accounts.UpdateAsync(account, cancellationToken);
            }

            await _trades.AddAsync(new Trade(Guid.NewGuid(), order.Id, order.Pair, order.Side, order.Quantity, fillPrice, now), cancellationToken);
        }
    }
}
