using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Handlers;

public sealed class PlaceLimitOrderHandler : IPlaceLimitOrderHandler
{
    private readonly IOrderRepository _orders;
    private readonly IClock _clock;

    public PlaceLimitOrderHandler(IOrderRepository orders, IClock clock)
    {
        _orders = orders;
        _clock = clock;
    }

    public async Task<OrderDto> HandleAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new LimitOrder(Guid.NewGuid(), request.Pair, request.Side, request.Quantity, request.LimitPrice, _clock.UtcNow);
        await _orders.AddAsync(order, cancellationToken);
        return OrderDto.From(order);
    }
}
