using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    // GET /api/dashboard/stats — role-aware
    [HttpGet("stats")]
    public IActionResult GetStats() =>
        Ok(_service.GetStats(CurrentUserId, CurrentRole));
}
