using System.Collections.Concurrent;
using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Infrastructure.Rates;

/// <summary>
/// Random-walk FX rate engine. newRate = oldRate * (1 + Δ), Δ ∈ [-0.001, +0.001].
/// Seeded with realistic May-2026 mid rates.
/// Thread-safe via per-pair locks; no I/O.
/// </summary>
public sealed class RandomWalkRateFeed : IRateFeed
{
    private static readonly IReadOnlyDictionary<CurrencyPair, decimal> DefaultSeeds =
        new Dictionary<CurrencyPair, decimal>
        {
            [CurrencyPair.EurUsd] = 1.16188m,
            [CurrencyPair.GbpUsd] = 1.33962m,
            [CurrencyPair.UsdChf] = 0.78716m,
        };

    private const decimal MaxDelta = 0.001m;

    private readonly IClock _clock;
    private readonly Random _random;
    private readonly object _lock = new();
    private readonly Dictionary<CurrencyPair, Rate> _current;

    public RandomWalkRateFeed(IClock clock, int? seed = null, IReadOnlyDictionary<CurrencyPair, decimal>? seeds = null)
    {
        _clock = clock;
        _random = seed is null ? new Random() : new Random(seed.Value);

        var initialSeeds = seeds ?? DefaultSeeds;
        var now = _clock.UtcNow;

        _current = initialSeeds.ToDictionary(entry => entry.Key, entry => new Rate(entry.Key, entry.Value, now));
    }

    public Rate CurrentRate(CurrencyPair pair)
    {
        lock (_lock) return _current[pair];
    }

    public IReadOnlyList<Rate> Snapshot()
    {
        lock (_lock) return _current.Values.ToList();
    }

    public Rate Advance(CurrencyPair pair)
    {
        lock (_lock)
        {
            var previousMid = _current[pair].Mid;
            // Δ ∈ [-MaxDelta, +MaxDelta]
            var delta = (decimal)((_random.NextDouble() * 2.0 - 1.0)) * MaxDelta;
            var nextMid = Math.Round(previousMid * (1m + delta), 6, MidpointRounding.AwayFromZero);
            
            if (nextMid <= 0m) nextMid = previousMid; // safety
            var rate = new Rate(pair, nextMid, _clock.UtcNow);
            _current[pair] = rate;

            return rate;
        }
    }
}
