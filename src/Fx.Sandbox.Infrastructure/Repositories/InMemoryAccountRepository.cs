using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Infrastructure.Repositories;

public sealed class InMemoryAccountRepository : IAccountRepository
{
    private Account _account = Account.NewSandbox();

    public Task<Account> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(_account);

    public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _account = account;
        return Task.CompletedTask;
    }
}
