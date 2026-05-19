using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Domain.Tests;

public class LimitOrderTests
{
    [Fact]
    public void Cannot_create_with_non_positive_quantity()
    {
        Action act = () => _ = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 0m, 1m, DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Cannot_create_with_non_positive_price()
    {
        Action act = () => _ = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 1m, 0m, DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Fill_transitions_to_filled_and_records_price_and_time()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m, DateTimeOffset.UtcNow);
        var t = DateTimeOffset.UtcNow;
        order.Fill(0.91m, t);
        order.Status.Should().Be(OrderStatus.Filled);
        order.FillPrice.Should().Be(0.91m);
        order.FilledAt.Should().Be(t);
    }

    [Fact]
    public void Cannot_fill_cancelled_order()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m, DateTimeOffset.UtcNow);
        order.Cancel(DateTimeOffset.UtcNow);
        Action act = () => order.Fill(0.91m, DateTimeOffset.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Modify_changes_quantity_and_price_only_when_open()
    {
        var order = new LimitOrder(Guid.NewGuid(), CurrencyPair.EurUsd, OrderSide.Buy, 100m, 0.92m, DateTimeOffset.UtcNow);
        order.Modify(150m, 0.93m);
        order.Quantity.Should().Be(150m);
        order.LimitPrice.Should().Be(0.93m);

        order.Fill(0.93m, DateTimeOffset.UtcNow);
        Action act = () => order.Modify(200m, 0.94m);
        act.Should().Throw<InvalidOperationException>();
    }
}
