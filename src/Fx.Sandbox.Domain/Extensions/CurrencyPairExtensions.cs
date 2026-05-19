namespace Fx.Sandbox.Domain;

public static class CurrencyPairExtensions
{
    public static string Code(this CurrencyPair pair) => pair switch
    {
        CurrencyPair.EurUsd => "EUR/USD",
        CurrencyPair.GbpUsd => "GBP/USD",
        CurrencyPair.UsdChf => "USD/CHF",
        _ => throw new ArgumentOutOfRangeException(nameof(pair), pair, null),
    };

    public static bool TryParse(string code, out CurrencyPair pair)
    {
        switch (code?.ToUpperInvariant())
        {
            case "EUR/USD" or "EURUSD":
                pair = CurrencyPair.EurUsd; return true;
            case "GBP/USD" or "GBPUSD":
                pair = CurrencyPair.GbpUsd; return true;
            case "USD/CHF" or "USDCHF":
                pair = CurrencyPair.UsdChf; return true;
            default:
                pair = default; return false;
        }
    }
}
