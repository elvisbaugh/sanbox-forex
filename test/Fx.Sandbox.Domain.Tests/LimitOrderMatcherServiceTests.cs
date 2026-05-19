using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Domain.Tests;

public class LimitOrderMatcherTests
{
    private static LimitOrder NewOrder(OrderSide side, decimal limit) =>
        new(Guid.NewGuid(), CurrencyPair.EurUsd, side, 100m, limit, DateTimeOffset.UtcNow);

    [Theory]
    [InlineData(0.92, 0.92, true)]   // buy at exactly the limit
    [InlineData(0.92, 0.91, true)]   // buy fills when mid drops below limit
    [InlineData(0.92, 0.93, false)]  // buy waits when mid above limit
    public void Buy_fills_when_mid_at_or_below_limit(decimal limit, decimal mid, bool expected)
    {
        var order = NewOrder(OrderSide.Buy, limit);
        LimitOrderMatcherService.ShouldFill(order, mid).Should().Be(expected);
    }

    [Theory]
    [InlineData(0.92, 0.92, true)]
    [InlineData(0.92, 0.93, true)]
    [InlineData(0.92, 0.91, false)]
    public void Sell_fills_when_mid_at_or_above_limit(decimal limit, decimal mid, bool expected)
    {
        var order = NewOrder(OrderSide.Sell, limit);
        LimitOrderMatcherService.ShouldFill(order, mid).Should().Be(expected);
    }

    [Fact]
    public void Non_open_orders_do_not_fill()
    {
        var order = NewOrder(OrderSide.Buy, 0.92m);
        order.Cancel(DateTimeOffset.UtcNow);
        LimitOrderMatcherService.ShouldFill(order, 0.50m).Should().BeFalse();
    }
}
