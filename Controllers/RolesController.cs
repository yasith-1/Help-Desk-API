using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ApiControllerBase
{
    private readonly IRoleService _service;
    public RolesController(IRoleService service) => _service = service;

    // GET /api/roles — roles.manage ඕන
    [HttpGet]
    public IActionResult GetAll()
    {
        RequirePermission(Permissions.RolesManage);
        return Ok(_service.GetAll());
    }

    // GET /api/roles/names — role නම් ලැයිස්තුව (user form dropdown එකට; users.manage ඕන)
    [HttpGet("names")]
    public IActionResult GetNames()
    {
        RequirePermission(Permissions.UsersManage);
        return Ok(_service.GetNames());
    }

    // GET /api/roles/permissions — permission catalog (matrix එකට)
    [HttpGet("permissions")]
    public IActionResult GetCatalog()
    {
        RequirePermission(Permissions.RolesManage);
        return Ok(_service.GetCatalog());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateRoleRequest request)
    {
        RequirePermission(Permissions.RolesManage);
        return Ok(_service.Create(request));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateRoleRequest request)
    {
        RequirePermission(Permissions.RolesManage);
        return Ok(_service.Update(id, request));
    }

    [HttpPut("{id}/permissions")]
    public IActionResult UpdatePermissions(int id, [FromBody] UpdatePermissionsRequest request)
    {
        RequirePermission(Permissions.RolesManage);
        return Ok(_service.UpdatePermissions(id, request.Permissions));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        RequirePermission(Permissions.RolesManage);
        _service.Delete(id);
        return NoContent();
    }
}
