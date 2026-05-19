using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IGetOrderBookQuery
{
    Task<IReadOnlyList<OrderDto>> HandleAsync(CancellationToken cancellationToken = default);
}
