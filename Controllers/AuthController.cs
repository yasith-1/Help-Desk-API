using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]                       // Spring: @RestController (+ auto validation)
[Route("api/auth")]                  // Spring: @RequestMapping("/api/auth")
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;
    private readonly IPermissionService _perms;

    public AuthController(IAuthService auth, IPermissionService perms)
    {
        _auth = auth;
        _perms = perms;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var result = _auth.Register(request);
        if (result == null)
            return Conflict(new { message = "username හෝ email එක දැනටමත් තියෙනවා" });   // 409
        return Ok(result);                                                               // 200 + token
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _auth.Login(request);
        if (result == null)
            return Unauthorized(new { message = "username හෝ password වැරදියි" });        // 401
        return Ok(result);                                                               // 200 + token
    }

    // GET /api/auth/permissions — current user ගේ permissions (UI refresh කරන්න)
    [HttpGet("permissions")]
    [Authorize]
    public IActionResult MyPermissions() =>
        Ok(_perms.GetPermissions(CurrentRole).ToList());
}
