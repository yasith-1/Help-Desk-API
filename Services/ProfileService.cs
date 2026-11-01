using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;
    public ProfileService(AppDbContext db) => _db = db;

    private User Load(int userId) =>
        _db.Users.Find(userId) ?? throw new KeyNotFoundException("User not found");

    public ProfileResponse Get(int userId) => ToResponse(Load(userId));

    public ProfileResponse Update(int userId, UpdateProfileRequest req)
    {
        var user = Load(userId);

        // Email unique — තමන්ව හැර වෙන කෙනෙකුට මේ email එක තියෙනවද බලනවා
        var email = req.Email.Trim();
        if (_db.Users.Any(u => u.Id != userId && u.Email == email))
            throw new ArgumentException("This email is already in use");

        user.FullName = req.FullName.Trim();
        user.Email = email;
        _db.SaveChanges();
        return ToResponse(user);
    }

    public void ChangePassword(int userId, ChangePasswordRequest req)
    {
        var user = Load(userId);

        // Current password හරිද කියලා verify කරනවා (වෙන කෙනෙක් password මාරු කරන එක නවතන්න)
        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        _db.SaveChanges();
    }

    public ProfileResponse SetAvatar(int userId, string avatarUrl)
    {
        var user = Load(userId);
        user.AvatarUrl = avatarUrl;
        _db.SaveChanges();
        return ToResponse(user);
    }

    public string? GetAvatarUrl(int userId) => Load(userId).AvatarUrl;

    private static ProfileResponse ToResponse(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Username = u.Username,
        Role = u.Role,
        AvatarUrl = u.AvatarUrl,
        CreatedAt = u.CreatedAt
    };
}
