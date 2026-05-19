using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IClosePositionHandler
{
    Task<OrderDto?> HandleAsync(CurrencyPair pair, CancellationToken cancellationToken = default);
}
