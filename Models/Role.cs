namespace ProductApi.Models;

// Role entity — admin ට custom roles හදන්න පුළුවන්. User.Role (string) = Role.Name.
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsSystem { get; set; } = false;   // built-in (Customer/Agent/Admin) — delete කරන්න බෑ
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public List<RolePermission> Permissions { get; set; } = new();
}
