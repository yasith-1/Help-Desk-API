using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

public class RoleResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsSystem { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int UserCount { get; set; }
}

public class CreateRoleRequest
{
    [Required(ErrorMessage = "role නම අනිවාර්යයි")]
    [StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [StringLength(200)]
    public string Description { get; set; } = "";

    public List<string> Permissions { get; set; } = new();
}

public class UpdateRoleRequest
{
    [Required][StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = "";
    [StringLength(200)]
    public string Description { get; set; } = "";
}

public class UpdatePermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}

public class PermissionItemResponse
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Group { get; set; } = "";
}
