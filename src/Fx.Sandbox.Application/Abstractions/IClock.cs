namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Abstraction over the system clock so time-dependent logic can be tested deterministically.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC instant.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
