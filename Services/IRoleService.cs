using HelpdeskApi.DTOs;

namespace HelpdeskApi.Services;

public interface IRoleService
{
    List<RoleResponse> GetAll();
    List<string> GetNames();
    List<PermissionItemResponse> GetCatalog();
    RoleResponse Create(CreateRoleRequest request);
    RoleResponse Update(int id, UpdateRoleRequest request);
    RoleResponse UpdatePermissions(int id, List<string> permissions);
    void Delete(int id);
}
