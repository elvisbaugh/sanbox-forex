namespace Fx.Sandbox.Domain;

public sealed class LimitOrder
{
    public Guid Id { get; }
    public CurrencyPair Pair { get; private set; }
    public OrderSide Side { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal LimitPrice { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? FilledAt { get; private set; }
    public decimal? FillPrice { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public LimitOrder(
        Guid id,
        CurrencyPair pair,
        OrderSide side,
        decimal quantity,
        decimal limitPrice,
        DateTimeOffset createdAt)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (limitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(limitPrice), "Limit price must be positive.");

        Id = id;
        Pair = pair;
        Side = side;
        Quantity = quantity;
        LimitPrice = limitPrice;
        Status = OrderStatus.Open;
        CreatedAt = createdAt;
    }

    public void Fill(decimal fillPrice, DateTimeOffset filledAt)
    {
        if (Status != OrderStatus.Open) throw new InvalidOperationException($"Cannot fill order in status {Status}.");
        if (fillPrice <= 0) throw new ArgumentOutOfRangeException(nameof(fillPrice));
        Status = OrderStatus.Filled;
        FillPrice = fillPrice;
        FilledAt = filledAt;
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        if (Status != OrderStatus.Open) throw new InvalidOperationException($"Cannot cancel order in status {Status}.");
        Status = OrderStatus.Cancelled;
        CancelledAt = cancelledAt;
    }

    public void Modify(decimal newQuantity, decimal newLimitPrice)
    {
        if (Status != OrderStatus.Open) throw new InvalidOperationException($"Cannot modify order in status {Status}.");

        if (newQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be positive.");

        if (newLimitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(newLimitPrice));

        Quantity = newQuantity;
        LimitPrice = newLimitPrice;
    }
}
