using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

// Authentication service — register, login, JWT token generation (Spring Security වගේ)
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IPermissionService _perms;

    public AuthService(AppDbContext db, IConfiguration config, IPermissionService perms)
    {
        _db = db;
        _config = config;
        _perms = perms;
    }

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
            Permissions = _perms.GetPermissions(user.Role).ToList(),
            ExpiresAt = expiresAt
        };
    }
}
