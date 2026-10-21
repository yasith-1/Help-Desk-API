namespace ProductApi.Models;

// Entity — users table. Password එක plain නෙවෙයි — HASH කරලා save කරනවා.
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";   // BCrypt hash (plain text කවදාවත් නෑ!)
    public string Role { get; set; } = Roles.Customer; // Customer / Agent / Admin
    public bool IsActive { get; set; } = true;         // deactivate කරොත් login බෑ
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (EF Core relationships)
    public List<Ticket> CreatedTickets { get; set; } = new();   // මේ user raise කරපු tickets
    public List<Ticket> AssignedTickets { get; set; } = new();  // මේ agent ට assign කරපු tickets
}
