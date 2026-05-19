using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IGetAccountSummaryQuery
{
    Task<AccountSummaryDto> HandleAsync(CancellationToken cancellationToken = default);
}
