namespace Fx.Sandbox.Domain;

public sealed record Trade(
    Guid Id,
    Guid OrderId,
    CurrencyPair Pair,
    OrderSide Side,
    decimal Quantity,
    decimal Price,
    DateTimeOffset ExecutedAt);
