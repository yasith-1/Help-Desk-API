namespace HelpdeskApi.Services;

// Email sending abstraction (Spring: a @Service wrapping JavaMailSender වගේ).
// SMTP configure කරලා නැත්නම් Development වලදී OTP එක console එකට log වෙනවා.
public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
