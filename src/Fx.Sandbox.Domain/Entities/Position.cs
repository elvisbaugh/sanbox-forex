namespace Fx.Sandbox.Domain;

/// <summary>
/// Net position for a single currency pair. Quantity is signed:
/// positive = long the base currency (USD), negative = short.
/// </summary>
public sealed class Position
{
    public CurrencyPair Pair { get; }
    public decimal Quantity { get; private set; }
    public decimal AveragePrice { get; private set; }
    public decimal RealisedPnL { get; private set; }

    public Position(CurrencyPair pair)
    {
        Pair = pair;
        Quantity = 0m;
        AveragePrice = 0m;
        RealisedPnL = 0m;
    }

    public bool IsFlat => Quantity == 0m;

    internal decimal Apply(OrderSide side, decimal fillQuantity, decimal fillPrice)
    {
        var signedFill = side == OrderSide.Buy ? fillQuantity : -fillQuantity;
        var newQuantity = Quantity + signedFill;

        if (Quantity == 0m)
        {
            // Opening a new position
            Quantity = newQuantity;
            AveragePrice = fillPrice;
            return 0m;
        }

        var sameDirection = Math.Sign(Quantity) == Math.Sign(signedFill);
        if (sameDirection)
        {
            // Increase: weighted-average price
            AveragePrice = ((Quantity * AveragePrice) + (signedFill * fillPrice)) / newQuantity;
            Quantity = newQuantity;
            return 0m;
        }

        // Opposite direction: reducing, closing, or flipping.
        var closingQuantity = Math.Min(Math.Abs(Quantity), Math.Abs(signedFill));
        // Realised P&L: per-unit difference between fill and average, scaled by signed-direction of existing position.
        var perUnitPnL = Quantity > 0
            ? (fillPrice - AveragePrice)   // long closed at fillPrice
            : (AveragePrice - fillPrice);  // short closed at fillPrice
        var realisedDelta = perUnitPnL * closingQuantity;
        RealisedPnL += realisedDelta;

        if (Math.Abs(signedFill) < Math.Abs(Quantity))
        {
            // Partial reduce; average price unchanged.
            Quantity = newQuantity;
        }
        else if (Math.Abs(signedFill) == Math.Abs(Quantity))
        {
            // Fully closed.
            Quantity = 0m;
            AveragePrice = 0m;
        }
        else
        {
            // Flip: remaining qty opens new position at fillPrice.
            Quantity = newQuantity;
            AveragePrice = fillPrice;
        }

        return realisedDelta;
    }
}
