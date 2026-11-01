using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

// Public self-registration — සෑම register එකක්ම Customer role එකෙන් පටන් ගන්නවා.
public class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Username is required")]
    [StringLength(30, MinimumLength = 3)]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public List<string> Permissions { get; set; } = new();   // role එකේ permissions
    public DateTime ExpiresAt { get; set; }
}
