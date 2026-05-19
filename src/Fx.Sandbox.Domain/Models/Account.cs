namespace Fx.Sandbox.Domain;

public sealed class Account
{
    public const decimal StartingCapitalUsd = 10_000m;

    public decimal Cash { get; private set; }
    public decimal RealisedPnL { get; private set; }

    public Account(decimal cash, decimal realisedPnL = 0m)
    {
        Cash = cash;
        RealisedPnL = realisedPnL;
    }

    public static Account NewSandbox() => new(StartingCapitalUsd);

    /// <summary>Books a realised P&amp;L delta into cash. Negative values are losses.</summary>
    public void Settle(decimal realisedDelta)
    {
        Cash += realisedDelta;
        RealisedPnL += realisedDelta;
    }
}
