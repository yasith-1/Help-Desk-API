using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services;

namespace ProductApi.Controllers;

// Base controller — JWT token එකේ claims වලින් current user id + role කියවනවා.
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    protected string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? "";

    // Permission එකක් නැත්නම් 403 (UnauthorizedAccessException → middleware).
    protected void RequirePermission(string permission)
    {
        var perms = HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        perms.Require(CurrentRole, permission);
    }
}
