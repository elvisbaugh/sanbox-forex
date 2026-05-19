using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

/// <summary>
/// Market-closes the entire net position for a pair at the current mid.
/// Creates a synthetic, already-filled limit order on the opposite side and
/// applies it to the position + trade log so the rest of the system stays
/// uniform.
/// </summary>
public sealed class ClosePositionHandler : IClosePositionHandler
{
    private readonly IPositionRepository _positions;
    private readonly IOrderRepository _orders;
    private readonly ITradeRepository _trades;
    private readonly IAccountRepository _accounts;
    private readonly IRateFeed _rates;
    private readonly IClock _clock;

    public ClosePositionHandler(
        IPositionRepository positions,
        IOrderRepository orders,
        ITradeRepository trades,
        IAccountRepository accounts,
        IRateFeed rates,
        IClock clock)
    {
        _positions = positions;
        _orders = orders;
        _trades = trades;
        _accounts = accounts;
        _rates = rates;
        _clock = clock;
    }

    public async Task<OrderDto?> HandleAsync(CurrencyPair pair, CancellationToken cancellationToken = default)
    {
        var position = await _positions.GetOrCreateAsync(pair, cancellationToken);
        if (position.IsFlat) return null;

        var closingSide = position.Quantity > 0 ? OrderSide.Sell : OrderSide.Buy;
        var quantity = Math.Abs(position.Quantity);
        var midPrice = _rates.CurrentRate(pair).Mid;
        var now = _clock.UtcNow;

        var order = new LimitOrder(Guid.NewGuid(), pair, closingSide, quantity, midPrice, now);
        order.Fill(midPrice, now);
        await _orders.AddAsync(order, cancellationToken);

        var realisedDelta = PositionCalculatorService.ApplyFill(position, closingSide, quantity, midPrice);
        await _positions.UpsertAsync(position, cancellationToken);

        if (realisedDelta != 0m)
        {
            var account = await _accounts.GetAsync(cancellationToken);
            account.Settle(realisedDelta);
            await _accounts.UpdateAsync(account, cancellationToken);
        }

        await _trades.AddAsync(new Trade(Guid.NewGuid(), order.Id, pair, closingSide, quantity, midPrice, now), cancellationToken);

        return OrderDto.From(order);
    }
}
