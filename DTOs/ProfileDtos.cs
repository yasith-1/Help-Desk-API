using System.ComponentModel.DataAnnotations;

namespace HelpdeskApi.DTOs;

// Logged-in user තමන්ගේ profile එක බලන/වෙනස් කරන DTOs.
// System fields (Id, Username, Role, IsActive) edit කරන්න දෙන්නේ නෑ — read-only විදිහට පෙන්නනවා.

public class ProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";   // read-only (login identity)
    public string Role { get; set; } = "";        // read-only
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Full Name + Email විතරක් edit කරන්න පුළුවන්.
public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = "";
}

// Current password confirm කරලා අලුත් password එකක් set කරනවා.
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "New password is required")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = "";
}
