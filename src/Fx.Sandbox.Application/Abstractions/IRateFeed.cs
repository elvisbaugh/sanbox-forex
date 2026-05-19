using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Source of live FX rates. Adapters can be random-walk, replay, or a real broker feed.
/// </summary>
public interface IRateFeed
{
    /// <summary>
    /// Returns the latest known rate for the given currency pair.
    /// </summary>
    Rate CurrentRate(CurrencyPair pair);

    /// <summary>
    /// Returns the latest known rate for every tracked pair.
    /// </summary>
    IReadOnlyList<Rate> Snapshot();

    /// <summary>
    /// Advances the feed one tick for the given pair and returns the new rate.
    /// </summary>
    Rate Advance(CurrencyPair pair);
}
