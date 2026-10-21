namespace ProductApi.Models;

// Role එකකට අයිති එක permission key එකක් (Role 1 — * RolePermission).
public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public string Permission { get; set; } = "";   // Permissions catalog එකේ key එකක්
}
