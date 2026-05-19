using Microsoft.AspNetCore.Mvc;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Api.Controllers;

[ApiController]
[Route("api/v1/trades")]
public sealed class TradesController : ControllerBase
{
    private readonly IGetTradesQuery _query;
    public TradesController(IGetTradesQuery query) => _query = query;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Trade>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Trade>>> Get([FromQuery] int? limit, CancellationToken cancellationToken) =>
        Ok(await _query.HandleAsync(limit, cancellationToken));
}
