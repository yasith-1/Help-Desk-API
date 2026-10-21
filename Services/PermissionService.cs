using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _db;
    public PermissionService(AppDbContext db) => _db = db;

    // Role එකේ permissions DB එකෙන් ගන්නවා. (කුඩා scale — cache නෑ; permission වෙනස් වෙද්දී
    // වහාම බලපානවා. ලොකු scale එකකට මෙය cache කරන්න පුළුවන්.)
    public HashSet<string> GetPermissions(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return new();
        return _db.RolePermissions
            .Where(p => p.Role!.Name == roleName && !p.Role.IsDeleted)
            .Select(p => p.Permission)
            .ToHashSet();
    }

    public bool Has(string roleName, string permission) =>
        GetPermissions(roleName).Contains(permission);

    public void Require(string roleName, string permission)
    {
        if (!Has(roleName, permission))
            throw new UnauthorizedAccessException("මේ ක්‍රියාව කරන්න ඔයාට අවසර නෑ");
    }
}
