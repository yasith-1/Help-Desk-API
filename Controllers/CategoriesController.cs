using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) => _service = service;

    // GET /api/categories — ඕනම login user කෙනෙකුට (ticket හදද්දී ඕන)
    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    // POST/PUT/DELETE — categories.manage permission ඕන
    [HttpPost]
    public IActionResult Create([FromBody] CategoryRequest request)
    {
        RequirePermission(Permissions.CategoriesManage);
        return Ok(_service.Create(request));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] CategoryRequest request)
    {
        RequirePermission(Permissions.CategoriesManage);
        return Ok(_service.Update(id, request));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        RequirePermission(Permissions.CategoriesManage);
        _service.Delete(id);
        return NoContent();
    }
}
