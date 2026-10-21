using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    // GET /api/users — users.manage
    [HttpGet]
    public IActionResult GetAll()
    {
        RequirePermission(Permissions.UsersManage);
        return Ok(_service.GetAll());
    }

    // GET /api/users/agents — assignment dropdown (tickets.assign තියෙන අයට)
    [HttpGet("agents")]
    public IActionResult GetAgents()
    {
        RequirePermission(Permissions.TicketsAssign);
        return Ok(_service.GetAgents());
    }

    // POST /api/users — users.manage
    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        RequirePermission(Permissions.UsersManage);
        return Ok(_service.Create(request));
    }

    [HttpPatch("{id}/role")]
    public IActionResult ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        RequirePermission(Permissions.UsersManage);
        return Ok(_service.ChangeRole(id, request.Role));
    }

    [HttpPatch("{id}/active")]
    public IActionResult SetActive(int id, [FromBody] SetActiveRequest request)
    {
        RequirePermission(Permissions.UsersManage);
        return Ok(_service.SetActive(id, request.IsActive));
    }
}
