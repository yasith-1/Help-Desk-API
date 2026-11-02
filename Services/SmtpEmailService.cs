using System.Net;
using System.Net.Mail;

namespace HelpdeskApi.Services;

// SMTP email sender (Gmail / Mailtrap / any SMTP).
// Config → appsettings "Email" section; password → User Secrets (plain text නෑ appsettings එකේ).
// SMTP configure කරලා නැත්නම් → email body එක logger එකට දානවා (dev වලදී OTP test කරන්න).
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _log;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> log)
    {
        _config = config;
        _log = log;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _config["Email:Host"];
        var user = _config["Email:User"];
        var pass = _config["Email:Password"];
        var enabled = _config.GetValue("Email:Enabled", false);

        // SMTP නැත්නම් / disable නම් → console fallback (dev testing)
        if (!enabled || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
        {
            _log.LogWarning(
                "📧 [EMAIL NOT SENT — SMTP disabled] To: {To} | Subject: {Subject}\n{Body}",
                toEmail, subject, StripHtml(htmlBody));
            return;
        }

        var port = _config.GetValue("Email:Port", 587);
        var from = _config["Email:From"];
        if (string.IsNullOrWhiteSpace(from)) from = user;
        var fromName = _config["Email:FromName"] ?? "HelpDesk";

        using var message = new MailMessage
        {
            From = new MailAddress(from!, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,                                  // STARTTLS (Gmail: 587)
            Credentials = new NetworkCredential(user, pass),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await client.SendMailAsync(message, ct);
        _log.LogInformation("📧 Email sent to {To} — {Subject}", toEmail, subject);
    }

    // log fallback එකට HTML tags අයින් කරනවා (console එකේ කියවන්න ලේසි)
    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ")
            .Replace("&nbsp;", " ").Trim();
}
