using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

// Public self-registration — සෑම register එකක්ම Customer role එකෙන් පටන් ගන්නවා.
public class RegisterRequest
{
    [Required(ErrorMessage = "නම අනිවාර්යයි")]
    [StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "email අනිවාර්යයි")]
    [EmailAddress(ErrorMessage = "වලංගු email එකක් දෙන්න")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "username අනිවාර්යයි")]
    [StringLength(30, MinimumLength = 3)]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "password අනිවාර්යයි")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "password අවම අකුරු 6")]
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
