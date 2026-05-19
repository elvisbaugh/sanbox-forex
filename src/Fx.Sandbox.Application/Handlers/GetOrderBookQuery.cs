using Fx.Sandbox.Application.Abstractions;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Handlers;

public sealed class GetOrderBookQuery : IGetOrderBookQuery
{
    private readonly IOrderRepository _orders;
    public GetOrderBookQuery(IOrderRepository orders) => _orders = orders;

    public async Task<IReadOnlyList<OrderDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var all = await _orders.ListAsync(cancellationToken);
        return all.OrderByDescending(order => order.CreatedAt).Select(OrderDto.From).ToList();
    }
}
