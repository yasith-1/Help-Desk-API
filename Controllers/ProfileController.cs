using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpdeskApi.DTOs;
using HelpdeskApi.Services;

namespace HelpdeskApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]                         // login ඕන — current user තමන්ගේ profile එකට විතරයි
public class ProfileController : ApiControllerBase
{
    private readonly IProfileService _profile;
    private readonly IWebHostEnvironment _env;

    // Upload rules — image විතරයි, 2MB දක්වා
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private static readonly Dictionary<string, string> AllowedTypes = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"]  = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"]  = ".gif",
    };

    public ProfileController(IProfileService profile, IWebHostEnvironment env)
    {
        _profile = profile;
        _env = env;
    }

    // GET /api/profile — තමන්ගේ profile එක
    [HttpGet]
    public IActionResult Get() => Ok(_profile.Get(CurrentUserId));

    // PUT /api/profile — Full Name + Email update
    [HttpPut]
    public IActionResult Update([FromBody] UpdateProfileRequest request) =>
        Ok(_profile.Update(CurrentUserId, request));

    // POST /api/profile/password — password මාරුව
    [HttpPost("password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        _profile.ChangePassword(CurrentUserId, request);
        return NoContent();
    }

    // POST /api/profile/avatar — multipart file upload (profile image)
    [HttpPost("avatar")]
    public IActionResult UploadAvatar(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was uploaded");
        if (file.Length > MaxAvatarBytes)
            throw new ArgumentException("Image must be 2MB or smaller");
        if (!AllowedTypes.TryGetValue(file.ContentType, out var ext))
            throw new ArgumentException("Only JPG, PNG, WebP or GIF images are allowed");

        // wwwroot/uploads/avatars folder එක හදනවා (නැත්නම්)
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "avatars");
        Directory.CreateDirectory(folder);

        // Unique filename — user-{id}-{guid}.ext
        var fileName = $"user-{CurrentUserId}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            file.CopyTo(stream);

        // පරණ avatar file එකක් තිබ්බොත් delete කරනවා (best-effort)
        DeleteOldAvatar(_profile.GetAvatarUrl(CurrentUserId), webRoot);

        // DB එකට relative URL එක save කරනවා (S3 future-ready — absolute URL නෙවෙයි)
        var url = $"/uploads/avatars/{fileName}";
        return Ok(_profile.SetAvatar(CurrentUserId, url));
    }

    private static void DeleteOldAvatar(string? oldUrl, string webRoot)
    {
        if (string.IsNullOrWhiteSpace(oldUrl) || !oldUrl.StartsWith("/uploads/avatars/")) return;
        try
        {
            var rel = oldUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(webRoot, rel);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
        catch { /* best-effort — delete fail උනත් අලුත් එක save වෙලා තියෙනවා */ }
    }
}
