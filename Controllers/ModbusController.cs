using Microsoft.AspNetCore.Mvc;
using ModbusRtuWebApi.Services;

namespace ModbusRtuWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModbusController : ControllerBase
{
    [HttpGet("live")]
    public IActionResult GetLive()
    {
        return Ok(ModbusPollingService.LiveData);
    }
}
