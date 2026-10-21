using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

// Admin විසින් account එකක් හදනවා (ඕනම role එකක්).
public class CreateUserRequest
{
    [Required][StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required][EmailAddress]
    public string Email { get; set; } = "";

    [Required][StringLength(30, MinimumLength = 3)]
    public string Username { get; set; } = "";

    [Required][StringLength(50, MinimumLength = 6)]
    public string Password { get; set; } = "";

    [Required] public string Role { get; set; } = "Customer";   // Customer / Agent / Admin
}

public class ChangeRoleRequest
{
    [Required] public string Role { get; set; } = "";
}

public class SetActiveRequest
{
    public bool IsActive { get; set; }
}

public class UserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Assignment dropdown වලට සැහැල්ලු version එක
public class AgentOption
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}
