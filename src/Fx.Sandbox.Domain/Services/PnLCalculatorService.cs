namespace Fx.Sandbox.Domain;

public static class PnLCalculatorService
{
    /// <summary>
    /// Unrealised P&amp;L for a position at the given mark price.
    /// Long: (mark - avg) * qty. Short: (avg - mark) * |qty|.
    /// </summary>
    public static decimal Unrealised(Position position, decimal markPrice)
    {
        if (position.IsFlat) return 0m;
        
        return position.Quantity > 0
            ? (markPrice - position.AveragePrice) * position.Quantity
            : (position.AveragePrice - markPrice) * Math.Abs(position.Quantity);
    }
}
