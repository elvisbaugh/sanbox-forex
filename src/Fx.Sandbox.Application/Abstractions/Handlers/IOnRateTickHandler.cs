using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IOnRateTickHandler
{
    Task HandleAsync(Rate rate, CancellationToken cancellationToken = default);
}
