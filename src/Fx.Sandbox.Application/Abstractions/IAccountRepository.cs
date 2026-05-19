using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions;

/// <summary>
/// Persistence port for the single sandbox <see cref="Account"/>.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Returns the current account snapshot (balance, realised P&amp;L, etc.).
    /// </summary>
    Task<Account> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new account snapshot, replacing the previous state.
    /// </summary>
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
}
