using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Handlers;

public sealed class CancelOrderHandler : ICancelOrderHandler
{
    private readonly IOrderRepository _orders;
    private readonly IClock _clock;

    public CancelOrderHandler(IOrderRepository orders, IClock clock)
    {
        _orders = orders;
        _clock = clock;
    }

    public async Task<OrderDto?> HandleAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetAsync(orderId, cancellationToken);
        if (order is null) return null;
        order.Cancel(_clock.UtcNow);
        await _orders.UpdateAsync(order, cancellationToken);
        return OrderDto.From(order);
    }
}
