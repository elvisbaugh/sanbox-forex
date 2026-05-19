using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Domain.Tests;

public class PnLCalculatorTests
{
    [Fact]
    public void Flat_position_has_zero_unrealised()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PnLCalculatorService.Unrealised(p, 0.92m).Should().Be(0m);
    }

    [Fact]
    public void Long_position_gains_when_mark_above_avg()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Buy, 100m, 0.90m);
        PnLCalculatorService.Unrealised(p, 1.00m).Should().Be(10m);
    }

    [Fact]
    public void Short_position_gains_when_mark_below_avg()
    {
        var p = new Position(CurrencyPair.EurUsd);
        PositionCalculatorService.ApplyFill(p, OrderSide.Sell, 100m, 1.00m);
        PnLCalculatorService.Unrealised(p, 0.90m).Should().Be(10m);
    }
}
