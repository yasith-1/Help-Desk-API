using ProductApi.DTOs;

namespace ProductApi.Services;

// Self-service profile — logged-in user තමන්ගේ details manage කරනවා (admin UserService එකෙන් වෙනස්).
public interface IProfileService
{
    ProfileResponse Get(int userId);
    ProfileResponse Update(int userId, UpdateProfileRequest request);   // Full Name + Email
    void ChangePassword(int userId, ChangePasswordRequest request);
    ProfileResponse SetAvatar(int userId, string avatarUrl);            // file save කලාට පස්සේ URL එක DB එකට
    string? GetAvatarUrl(int userId);                                   // පරණ file එක delete කරන්න
}
