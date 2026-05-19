using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Short-lived in-memory ring buffer of recent rates per pair, used to power chart endpoints.
/// </summary>
public interface IRateHistory
{
    /// <summary>
    /// Records a new rate tick at the head of the buffer.
    /// </summary>
    void Append(Rate rate);

    /// <summary>
    /// Returns up to <paramref name="count"/> most-recent ticks for the given pair, oldest first.
    /// </summary>
    IReadOnlyList<Rate> Recent(CurrencyPair pair, int count);
}
