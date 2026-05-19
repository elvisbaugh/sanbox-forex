using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fx.Sandbox.Infrastructure.Rates;

/// <summary>
/// Drives the rate engine once per second: advances each pair's mid,
/// appends to history, then runs the matcher against resting orders.
/// All work is serialised through a single SemaphoreSlim so the matcher
/// sees a consistent view of orders + positions.
/// </summary>
public sealed class RateTickBackgroundService : BackgroundService
{
    private readonly IRateFeed _feed;
    private readonly IRateHistory _history;
    private readonly IOnRateTickHandler _onTick;
    private readonly ILogger<RateTickBackgroundService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

    public RateTickBackgroundService(
        IRateFeed feed,
        IRateHistory history,
        IOnRateTickHandler onTick,
        ILogger<RateTickBackgroundService> logger)
    {
        _feed = feed;
        _history = history;
        _onTick = onTick;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Seed history with the initial rates.
        foreach (var r in _feed.Snapshot()) _history.Append(r);

        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await _gate.WaitAsync(stoppingToken);
                try
                {
                    foreach (var pair in Enum.GetValues<CurrencyPair>())
                    {
                        var rate = _feed.Advance(pair);
                        _history.Append(rate);
                        await _onTick.HandleAsync(rate, stoppingToken);
                    }
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rate tick loop failed");
            }
        }
    }
}
