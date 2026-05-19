using Microsoft.AspNetCore.Mvc;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Api.Controllers;

[ApiController]
[Route("api/v1/positions")]
public class PositionsController(IGetPositionsQuery query, IClosePositionHandler close) : ControllerBase
{
    private readonly IGetPositionsQuery _query = query;
    private readonly IClosePositionHandler _close = close;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PositionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PositionDto>>> Get(CancellationToken cancellationToken) =>
        Ok(await _query.HandleAsync(cancellationToken));
        

    /// <summary>
    /// Market-close the entire net position for a pair at the current mid.
    /// </summary>
    [HttpPost("{pair}/close")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> Close(string pair, CancellationToken cancellationToken)
    {
        if (!CurrencyPairExtensions.TryParse(pair, out var parsedPair))
            return Problem(statusCode: 400, title: "Unknown pair", detail: pair);

        var order = await _close.HandleAsync(parsedPair, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
