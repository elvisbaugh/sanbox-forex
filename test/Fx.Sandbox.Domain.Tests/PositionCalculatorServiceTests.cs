using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Domain.Tests;

public class PositionCalculatorTests
{
    [Fact]
    public void Opening_long_sets_qty_and_avg_price()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.92m);
        p.Quantity.Should().Be(100m);
        p.AveragePrice.Should().Be(0.92m);
        p.RealisedPnL.Should().Be(0m);
    }

    [Fact]
    public void Increasing_long_weighted_averages_price()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.90m);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 1.00m);
        p.Quantity.Should().Be(200m);
        p.AveragePrice.Should().Be(0.95m);
    }

    [Fact]
    public void Partially_reducing_long_realises_proportional_pnl()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.90m);
        PositionCalculatorService.ApplyFill(p, OrderSide.Sell, 40m, 1.00m);
        p.Quantity.Should().Be(60m);
        p.AveragePrice.Should().Be(0.90m);          // unchanged on reduce
        p.RealisedPnL.Should().Be(40m * 0.10m);     // 4.0
    }

    [Fact]
    public void Fully_closing_long_realises_full_pnl_and_flattens()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.90m);
        PositionCalculatorService.ApplyFill(p, OrderSide.Sell, 100m, 1.00m);
        p.IsFlat.Should().BeTrue();
        p.RealisedPnL.Should().Be(10m);
        p.AveragePrice.Should().Be(0m);
    }

    [Fact]
    public void Flipping_long_to_short_realises_close_and_opens_short_at_fill_price()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.90m);
        PositionCalculatorService.ApplyFill(p, OrderSide.Sell, 150m, 1.00m);
        p.Quantity.Should().Be(-50m);
        p.AveragePrice.Should().Be(1.00m);
        p.RealisedPnL.Should().Be(100m * 0.10m);
    }

    [Fact]
    public void Opening_short_then_closing_realises_positive_pnl_when_price_falls()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Sell, 100m, 1.00m);
        p.Quantity.Should().Be(-100m);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.90m);
        p.IsFlat.Should().BeTrue();
        p.RealisedPnL.Should().Be(10m);
    }

    [Fact]
    public void Rejects_non_positive_qty_or_price()
    {
        var p = new Position(CurrencyPair.EurUsd);
        Action a1 = () => PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 0m, 1m);
        Action a2 = () => PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 1m, 0m);
        a1.Should().Throw<ArgumentOutOfRangeException>();
        a2.Should().Throw<ArgumentOutOfRangeException>();
    }
}
