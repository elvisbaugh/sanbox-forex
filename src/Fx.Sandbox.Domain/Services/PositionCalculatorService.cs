namespace Fx.Sandbox.Domain;

public static class PositionCalculatorService
{
    public static decimal ApplyFill(Position position, OrderSide side, decimal fillQuantity, decimal fillPrice)
    {
        if (fillQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(fillQuantity));

        if (fillPrice <= 0) throw new ArgumentOutOfRangeException(nameof(fillPrice));
        return position.Apply(side, fillQuantity, fillPrice);
    }
}
