using HelpdeskApi.DTOs;

namespace HelpdeskApi.Services;

public interface IAuthService
{
    AuthResponse? Register(RegisterRequest request);   // username/email දැනටමත් තියෙනවා නම් → null
    AuthResponse? Login(LoginRequest request);         // වැරදි credentials / inactive → null
}
