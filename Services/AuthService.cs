using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using HelpdeskApi.Data;
using HelpdeskApi.DTOs;
using HelpdeskApi.Models;

namespace HelpdeskApi.Services;

// Authentication service — register, login, JWT token generation (Spring Security වගේ)
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IPermissionService _perms;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _email;
    private readonly ILogger<AuthService> _log;

    // OTP settings
    private const int OtpTtlMinutes = 5;     // OTP එක valid වෙන කාලය
    private const int MaxAttempts = 5;       // වැරදි OTP tries — ඊට පස්සේ invalidate

    public AuthService(AppDbContext db, IConfiguration config, IPermissionService perms,
        IMemoryCache cache, IEmailService email, ILogger<AuthService> log)
    {
        _db = db;
        _config = config;
        _perms = perms;
        _cache = cache;
        _email = email;
        _log = log;
    }

    // cache එකේ OTP entry එක (OTP එක hash කරලා තියෙනවා — plain නෑ)
    private sealed class OtpEntry { public string Hash = ""; public int Attempts; }
    private static string CacheKey(string email) => $"pwdreset:{email.Trim().ToLowerInvariant()}";

    public AuthResponse? Register(RegisterRequest request)
    {
        // username/email unique check
        if (_db.Users.Any(u => u.Username == request.Username || u.Email == request.Email))
            return null;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),  // HASH (plain නෑ!)
            Role = Roles.Customer                                             // self-register → Customer
        };
        _db.Users.Add(user);
        _db.SaveChanges();

        return GenerateToken(user);
    }

    public AuthResponse? Login(LoginRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == request.Username);

        // user නෑ / inactive / password match නෑ → null (login fail)
        if (user == null || !user.IsActive ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return GenerateToken(user);
    }

    // ── Forgot password: Step 1 — OTP generate + email ───────────────────────────
    // return: true = account හිටියා, OTP යැව්වා. false = එහෙම (active) account එකක් නෑ.
    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email);

        // user නෑ / inactive → OTP යවන්නේ නෑ (controller එක 404 දෙනවා)
        if (user == null || !user.IsActive)
        {
            _log.LogInformation("Password reset requested for unknown/inactive email {Email} — no OTP sent.", email);
            return false;
        }

        // crypto-strong 6-digit OTP (000000–999999)
        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        _cache.Set(
            CacheKey(email),
            new OtpEntry { Hash = BCrypt.Net.BCrypt.HashPassword(otp), Attempts = 0 },
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpTtlMinutes) });

        await _email.SendAsync(user.Email, "Your HelpDesk password reset code", BuildOtpEmail(user.FullName, otp));
        return true;
    }

    // ── Step 2 — OTP valid ද කියලා check (reset form එකට යන්න කලින්) ──────────────
    public bool VerifyOtp(string email, string otp)
    {
        if (!_cache.TryGetValue(CacheKey(email), out OtpEntry? entry) || entry == null)
            return false;

        if (entry.Attempts >= MaxAttempts) { _cache.Remove(CacheKey(email)); return false; }

        if (BCrypt.Net.BCrypt.Verify(otp, entry.Hash)) return true;

        entry.Attempts++;   // reference object — cache එකේ update වෙනවා (expiry එක එහෙමමයි)
        return false;
    }

    // ── Step 3 — OTP නැවත verify කරලා password reset ──────────────────────────────
    public (bool ok, string? error) ResetPassword(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return (false, "Passwords do not match");

        if (!_cache.TryGetValue(CacheKey(request.Email), out OtpEntry? entry) || entry == null)
            return (false, "The code has expired or is invalid. Please request a new one.");

        if (entry.Attempts >= MaxAttempts)
        {
            _cache.Remove(CacheKey(request.Email));
            return (false, "Too many incorrect attempts. Please request a new code.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Otp, entry.Hash))
        {
            entry.Attempts++;
            return (false, "Invalid code. Please check and try again.");
        }

        var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null || !user.IsActive)
            return (false, "The code has expired or is invalid. Please request a new one.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _db.SaveChanges();

        _cache.Remove(CacheKey(request.Email));   // OTP එක consume කරනවා (නැවත use කරන්න බෑ)
        _log.LogInformation("Password reset successful for user {UserId} ({Email}).", user.Id, user.Email);
        return (true, null);
    }

    // OTP email body (simple inline-styled HTML — email clients වලට safe)
    private static string BuildOtpEmail(string name, string otp) => $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;padding:24px"">
  <h2 style=""margin:0 0 4px"">Password reset</h2>
  <p style=""color:#555"">Hi {System.Net.WebUtility.HtmlEncode(name)}, use this code to reset your HelpDesk password:</p>
  <div style=""font-size:32px;font-weight:700;letter-spacing:8px;text-align:center;
              background:#f4f4f5;border-radius:10px;padding:16px;margin:16px 0"">{otp}</div>
  <p style=""color:#777;font-size:13px"">This code expires in {OtpTtlMinutes} minutes. If you didn't request this, you can safely ignore this email.</p>
</div>";

    // JWT token එකක් හදනවා (Header.Payload.Signature)
    private AuthResponse GenerateToken(User user)
    {
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // claims = token එකේ payload (user info — DB lookup නැතුව කියවන්න පුළුවන්)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)              // role → authorization වලට
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            Permissions = _perms.GetPermissions(user.Role).ToList(),
            ExpiresAt = expiresAt
        };
    }
}
