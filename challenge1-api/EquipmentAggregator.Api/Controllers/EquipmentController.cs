using EquipmentAggregator.Api.Models;
using EquipmentAggregator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentAggregator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly EquipmentService _service;

    public EquipmentController(EquipmentService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<EquipmentResponse>> Get(
        [FromQuery] bool? available,
        [FromQuery] decimal? maxDailyRate,
        CancellationToken ct)
    {
        if (maxDailyRate < 0)
            return BadRequest("maxDailyRate must be zero or greater.");

        return Ok(await _service.GetEquipmentAsync(available, maxDailyRate, ct));
    }
}
