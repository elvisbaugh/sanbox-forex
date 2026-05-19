using Microsoft.AspNetCore.Mvc;

namespace Fx.Sandbox.Api.Controllers;

[ApiController]
[Route("healthz")]
public class HealthController : ControllerBase
{
    [HttpGet] public IActionResult Get() => Ok(new { status = "ok" });
}
