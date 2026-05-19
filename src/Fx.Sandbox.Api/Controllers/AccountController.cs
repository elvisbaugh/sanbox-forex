using Microsoft.AspNetCore.Mvc;
using Fx.Sandbox.Application.Abstractions.Handlers;
using Fx.Sandbox.Application.Dtos;

namespace Fx.Sandbox.Api.Controllers;

[ApiController]
[Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly IGetAccountSummaryQuery _query;
    public AccountController(IGetAccountSummaryQuery query) => _query = query;

    [HttpGet]
    [ProducesResponseType(typeof(AccountSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountSummaryDto>> Get(CancellationToken cancellationToken) =>
        Ok(await _query.HandleAsync(cancellationToken));
}
