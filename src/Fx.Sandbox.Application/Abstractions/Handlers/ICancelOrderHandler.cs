using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface ICancelOrderHandler
{
    Task<OrderDto?> HandleAsync(Guid orderId, CancellationToken cancellationToken = default);
}
