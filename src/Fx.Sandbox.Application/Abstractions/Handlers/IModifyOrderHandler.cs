using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IModifyOrderHandler
{
    Task<OrderDto?> HandleAsync(Guid orderId, ModifyOrderRequest request, CancellationToken cancellationToken = default);
}
