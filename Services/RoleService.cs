using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;
    public RoleService(AppDbContext db) => _db = db;

    public List<RoleResponse> GetAll()
    {
        var roles = _db.Roles.Include(r => r.Permissions).Where(r => !r.IsDeleted).OrderBy(r => r.Id).ToList();
        // user counts (role name එකෙන්)
        var counts = _db.Users.GroupBy(u => u.Role).Select(g => new { g.Key, C = g.Count() }).ToDictionary(x => x.Key, x => x.C);
        return roles.Select(r => ToResponse(r, counts.GetValueOrDefault(r.Name, 0))).ToList();
    }

    public List<string> GetNames() =>
        _db.Roles.Where(r => !r.IsDeleted).OrderBy(r => r.Id).Select(r => r.Name).ToList();

    public List<PermissionItemResponse> GetCatalog() =>
        Permissions.Catalog.Select(c => new PermissionItemResponse { Key = c.Key, Label = c.Label, Group = c.Group }).ToList();

    public RoleResponse Create(CreateRoleRequest req)
    {
        if (_db.Roles.Any(r => r.Name == req.Name && !r.IsDeleted))
            throw new ArgumentException("මේ නමින් role එකක් දැනටමත් තියෙනවා");

        var keys = CleanPermissions(req.Permissions);
        var role = new Role
        {
            Name = req.Name.Trim(),
            Description = req.Description,
            IsSystem = false,
            Permissions = keys.Select(k => new RolePermission { Permission = k }).ToList()
        };
        _db.Roles.Add(role);
        _db.SaveChanges();
        return ToResponse(role, 0);
    }

    public RoleResponse Update(int id, UpdateRoleRequest req)
    {
        var role = Load(id);
        if (role.IsSystem && role.Name != req.Name)
            throw new ArgumentException("System role එකක නම වෙනස් කරන්න බෑ");
        if (_db.Roles.Any(r => r.Name == req.Name && r.Id != id && !r.IsDeleted))
            throw new ArgumentException("මේ නමින් role එකක් දැනටමත් තියෙනවා");

        var oldName = role.Name;
        role.Name = req.Name.Trim();
        role.Description = req.Description;

        // role එකේ නම වෙනස් උනොත්, ඒ role එක තියෙන users ලාගේ Role string එකත් update කරනවා
        if (oldName != role.Name)
            foreach (var u in _db.Users.Where(u => u.Role == oldName)) u.Role = role.Name;

        _db.SaveChanges();
        return ToResponse(role, _db.Users.Count(u => u.Role == role.Name));
    }

    public RoleResponse UpdatePermissions(int id, List<string> permissions)
    {
        var role = Load(id);

        // Admin role එකට හැම විටම සියලු permissions — lock කරලා ( අහම්බෙන් lock out වෙන එක නවතන්න)
        var keys = role.Name == Models.Roles.Admin ? Permissions.All.ToList() : CleanPermissions(permissions);

        _db.RolePermissions.RemoveRange(role.Permissions);
        role.Permissions = keys.Select(k => new RolePermission { RoleId = role.Id, Permission = k }).ToList();
        _db.SaveChanges();
        return ToResponse(role, _db.Users.Count(u => u.Role == role.Name));
    }

    // Soft delete — IsDeleted flag (system / in-use roles delete කරන්න බෑ).
    public void Delete(int id)
    {
        var role = Load(id);
        if (role.IsSystem) throw new ArgumentException("System role එකක් මකන්න බෑ");
        if (_db.Users.Any(u => u.Role == role.Name))
            throw new ArgumentException("Users ලා සිටින role එකක් මකන්න බෑ (මුලින් ඒ users ලා වෙන role එකකට මාරු කරන්න)");

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        _db.SaveChanges();
    }

    // ── helpers ──
    private Role Load(int id) =>
        _db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.Id == id && !r.IsDeleted)
        ?? throw new KeyNotFoundException($"Role {id} හමු නොවීය");

    private static List<string> CleanPermissions(IEnumerable<string> perms) =>
        perms.Where(Permissions.IsValid).Distinct().ToList();

    private static RoleResponse ToResponse(Role r, int userCount) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        IsSystem = r.IsSystem,
        Permissions = r.Permissions.Select(p => p.Permission).ToList(),
        UserCount = userCount
    };
}
