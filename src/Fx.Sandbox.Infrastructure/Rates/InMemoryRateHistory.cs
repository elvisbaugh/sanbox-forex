using System.Collections.Concurrent;
using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Infrastructure.Rates;

/// <summary>
/// Fixed-size ring buffer of recent rates per currency pair.
/// </summary>
public sealed class InMemoryRateHistory : IRateHistory
{
    private readonly int _capacity;
    private readonly ConcurrentDictionary<CurrencyPair, Queue<Rate>> _buffers = new();

    public InMemoryRateHistory(int capacity = 300)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
    }

    public void Append(Rate rate)
    {
        var queue = _buffers.GetOrAdd(rate.Pair, _ => new Queue<Rate>(_capacity));
        lock (queue)
        {
            queue.Enqueue(rate);
            while (queue.Count > _capacity) queue.Dequeue();
        }
    }

    public IReadOnlyList<Rate> Recent(CurrencyPair pair, int count)
    {
        if (!_buffers.TryGetValue(pair, out var queue)) return Array.Empty<Rate>();
        lock (queue)
        {
            if (count >= queue.Count) return queue.ToArray();
            return queue.Skip(queue.Count - count).ToArray();
        }
    }
}
