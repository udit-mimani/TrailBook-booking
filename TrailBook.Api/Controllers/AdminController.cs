using TrailBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace TrailBook.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("trips/{id}/metrics")]
    public async Task<IActionResult> GetTripMetrics(Guid id)
    {
        try
        {
            var metrics = await _adminService.GetTripMetricsAsync(id);
            return Ok(metrics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("trips/at-risk")]
    public async Task<IActionResult> GetAtRiskTrips()
    {
        var trips = await _adminService.GetAtRiskTripsAsync();
        return Ok(new { at_risk_trips = trips });
    }
}