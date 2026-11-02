using System.ComponentModel.DataAnnotations;

namespace HelpdeskApi.DTOs;

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

// ── Forgot password flow (3 steps: request OTP → verify OTP → reset) ──────────────

// Step 1: user දුන්න email එකට OTP එකක් යවන්න.
public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = "";
}

// Step 2: email + OTP එක valid ද කියලා check කරනවා (reset page එකට යන්න කලින්).
public class VerifyOtpRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "OTP is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = "";
}

// Step 3: OTP එක නැවත verify කරලා අලුත් password එක save කරනවා.
public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "OTP is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = "";
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public string? AvatarUrl { get; set; }                   // profile image (null = initials)
    public List<string> Permissions { get; set; } = new();   // role එකේ permissions
    public DateTime ExpiresAt { get; set; }
}
