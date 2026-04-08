using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CarBazzar.Services;

/// <summary>
/// Reusable SMTP email service that reads credentials from appsettings.json.
/// Uses MailKit for robust connectivity, avoiding legacy System.Net.Mail timeouts.
/// </summary>
public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Sends an HTML email asynchronously via Gmail SMTP using MailKit.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="htmlBody">HTML body content.</param>
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var section = _config.GetSection("EmailSettings");

        var fromEmail = section["Email"]!;
        var password  = section["Password"]!;
        var host      = section["Host"]!;
        var port      = int.Parse(section["Port"]!);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("CarBazzar", fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        
        // Connect automatically selecting proper SSL/TLS depending on the port (587 usually maps to StartTls)
        await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
        
        // Authenticate using the App Password
        await client.AuthenticateAsync(fromEmail, password);
        
        // Send and gracefully disconnect
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
