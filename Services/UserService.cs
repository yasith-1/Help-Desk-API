using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPermissionService _perms;
    public UserService(AppDbContext db, IPermissionService perms) { _db = db; _perms = perms; }

    private bool RoleExists(string role) => _db.Roles.Any(r => r.Name == role);

    public List<UserResponse> GetAll() =>
        _db.Users.OrderBy(u => u.FullName).Select(ToResponse).ToList();

    // Assign කරන්න පුළුවන් අය = tickets handle කරන (viewAll permission තියෙන) roles වල active users
    public List<AgentOption> GetAgents()
    {
        var staffRoles = _db.RolePermissions
            .Where(p => p.Permission == Permissions.TicketsViewAll)
            .Select(p => p.Role!.Name).Distinct().ToList();

        return _db.Users
           .Where(u => u.IsActive && staffRoles.Contains(u.Role))
           .OrderBy(u => u.FullName)
           .Select(u => new AgentOption { Id = u.Id, FullName = u.FullName, Role = u.Role })
           .ToList();
    }

    public UserResponse Create(CreateUserRequest req)
    {
        if (!RoleExists(req.Role))
            throw new ArgumentException("වලංගු role එකක් තෝරන්න");
        if (_db.Users.Any(u => u.Username == req.Username || u.Email == req.Email))
            throw new ArgumentException("username හෝ email එක දැනටමත් තියෙනවා");

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            Username = req.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role,
            IsActive = true
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return ToResponse(user);
    }

    public UserResponse ChangeRole(int id, string role)
    {
        if (!RoleExists(role))
            throw new ArgumentException("වලංගු role එකක් තෝරන්න");

        var user = _db.Users.Find(id)
            ?? throw new KeyNotFoundException($"User {id} හමු නොවීය");
        user.Role = role;
        _db.SaveChanges();
        return ToResponse(user);
    }

    public UserResponse SetActive(int id, bool isActive)
    {
        var user = _db.Users.Find(id)
            ?? throw new KeyNotFoundException($"User {id} හමු නොවීය");
        user.IsActive = isActive;
        _db.SaveChanges();
        return ToResponse(user);
    }

    private static UserResponse ToResponse(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Username = u.Username,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
