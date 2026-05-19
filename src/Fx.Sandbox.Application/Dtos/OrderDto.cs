using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Dtos;

public sealed record OrderDto(
    Guid Id,
    CurrencyPair Pair,
    OrderSide Side,
    decimal Quantity,
    decimal LimitPrice,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? FilledAt,
    decimal? FillPrice,
    DateTimeOffset? CancelledAt)
{
    public static OrderDto From(LimitOrder order) =>
        new(
            order.Id, 
            order.Pair, 
            order.Side, 
            order.Quantity, 
            order.LimitPrice, 
            order.Status, 
            order.CreatedAt, 
            order.FilledAt, 
            order.FillPrice, 
            order.CancelledAt
        );
}
