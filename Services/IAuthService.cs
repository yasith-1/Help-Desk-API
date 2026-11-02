using HelpdeskApi.DTOs;

namespace HelpdeskApi.Services;

public interface IAuthService
{
    AuthResponse? Register(RegisterRequest request);   // username/email දැනටමත් තියෙනවා නම් → null
    AuthResponse? Login(LoginRequest request);         // වැරදි credentials / inactive → null

    // ── Forgot password (OTP) ────────────────────────────────────────────────────
    // Step 1: OTP එකක් generate කරලා email කරනවා. return false = එහෙම (active) account එකක් නෑ.
    Task<bool> RequestPasswordResetAsync(string email);
    // Step 2: OTP එක තාම valid ද? (reset form එකට යන්න කලින් UI check එක)
    bool VerifyOtp(string email, string otp);
    // Step 3: OTP එක නැවත verify කරලා password එක reset කරනවා. (ok, error-message)
    (bool ok, string? error) ResetPassword(ResetPasswordRequest request);
}
