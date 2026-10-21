namespace ProductApi.Services;

// Role name එකකින් ඒ role එකේ permissions resolve කරනවා (DB එකෙන්).
public interface IPermissionService
{
    HashSet<string> GetPermissions(string roleName);
    bool Has(string roleName, string permission);
    void Require(string roleName, string permission);   // නැත්නම් UnauthorizedAccessException → 403
}
