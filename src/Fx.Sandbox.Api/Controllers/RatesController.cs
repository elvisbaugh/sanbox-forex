using Microsoft.AspNetCore.Mvc;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;
using Fx.Sandbox.Domain;

namespace Fx.Sandbox.Api.Controllers;

[ApiController]
[Route("api/v1/rates")]
public class RatesController(IGetCurrentRatesQuery current, IGetRateHistoryQuery history) : ControllerBase
{
    private readonly IGetCurrentRatesQuery _current = current;
    private readonly IGetRateHistoryQuery _history = history;
    

    /// <summary>
    /// Latest mid rates for all simulated pairs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RateDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<RateDto>> Get() => Ok(_current.Handle());


    /// <summary>
    /// Recent rate history for a pair (most recent last).
    /// </summary>
    [HttpGet("{pair}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<RateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<RateDto>> GetHistory(string pair, [FromQuery] int count = 60)
    {
        if (!CurrencyPairExtensions.TryParse(pair, out var parsed))
            return Problem(statusCode: 404, title: "Unknown currency pair", detail: pair);

        if (count <= 0 || count > 1000)
            return Problem(statusCode: 400, title: "Invalid count", detail: "count must be in 1..1000");
            
        return Ok(_history.Handle(parsed, count));
    }
}
