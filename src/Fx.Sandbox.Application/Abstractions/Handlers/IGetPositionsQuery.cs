using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Application.Abstractions.Handlers;

public interface IGetPositionsQuery
{
    Task<IReadOnlyList<PositionDto>> HandleAsync(CancellationToken cancellationToken = default);
}
