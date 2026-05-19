using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IPlaceLimitOrderHandler
{
    Task<OrderDto> HandleAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default);
}
