using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpdeskApi.DTOs;
using HelpdeskApi.Services;

namespace HelpdeskApi.Controllers;

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
            return Conflict(new { message = "Username or email already exists" });   // 409
        return Ok(result);                                                               // 200 + token
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _auth.Login(request);
        if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });        // 401
        return Ok(result);                                                               // 200 + token
    }

    // POST /api/auth/forgot-password — email එකට OTP එකක් යවනවා (6 digits, 5 min)
    // Email register වෙලා නැත්නම් 404 → UI එකේ "No account found" පෙන්නනවා (Option B).
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var sent = await _auth.RequestPasswordResetAsync(request.Email);
        if (!sent)
            return NotFound(new { message = "No account found with this email" });   // 404
        return Ok(new { message = "A reset code has been sent to your email." });
    }

    // POST /api/auth/verify-otp — OTP එක valid ද කියලා check (reset form එකට යන්න කලින්)
    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!_auth.VerifyOtp(request.Email, request.Otp))
            return BadRequest(new { message = "Invalid or expired code" });   // 400
        return Ok(new { valid = true });
    }

    // POST /api/auth/reset-password — OTP + new password → password reset
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var (ok, error) = _auth.ResetPassword(request);
        if (!ok)
            return BadRequest(new { message = error });                       // 400
        return Ok(new { message = "Password reset successful. You can now sign in." });
    }

    // GET /api/auth/permissions — current user ගේ permissions (UI refresh කරන්න)
    [HttpGet("permissions")]
    [Authorize]
    public IActionResult MyPermissions() =>
        Ok(_perms.GetPermissions(CurrentRole).ToList());
}
