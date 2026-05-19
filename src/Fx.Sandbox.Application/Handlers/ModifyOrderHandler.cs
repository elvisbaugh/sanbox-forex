using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Handlers;

public sealed class ModifyOrderHandler : IModifyOrderHandler
{
    private readonly IOrderRepository _orders;

    public ModifyOrderHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<OrderDto?> HandleAsync(Guid orderId, ModifyOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetAsync(orderId, cancellationToken);

        if (order is null) return null;
        
        order.Modify(request.Quantity, request.LimitPrice);
        await _orders.UpdateAsync(order, cancellationToken);

        return OrderDto.From(order);
    }
}
