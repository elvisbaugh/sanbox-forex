namespace Fx.Sandbox.Domain;

/// <summary>
/// Decides whether a resting limit order should fill given the current mid price.
/// Mid-price matching, zero spread (documented assumption).
/// </summary>
public static class LimitOrderMatcherService
{
    public static bool ShouldFill(LimitOrder order, decimal currentMid)
    {
        if (order.Status != OrderStatus.Open) return false;
        
        return order.Side switch
        {
            OrderSide.Buy => currentMid <= order.LimitPrice,
            OrderSide.Sell => currentMid >= order.LimitPrice,
            _ => false,
        };
    }
}
