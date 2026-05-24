using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class EmailPlugin(IConfiguration configuration, ILogger<EmailPlugin> logger)
{
    [KernelFunction("send_email")]
    [Description("Sends a real email using SMTP")]
    public async Task<string> SendEmailAsync(
        [Description("Target email address")] string to,
        [Description("Email subject")] string subject,
        [Description("Email body content")] string content)
    {
        var smtpHost = configuration["SMTP_HOST"];
        var smtpPort = configuration["SMTP_PORT"];
        var smtpUser = configuration["SMTP_USER"];
        var smtpPass = configuration["SMTP_PASS"];

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser))
        {
            logger.LogWarning("EmailPlugin: SMTP not configured. Simulating send.");
            return $"[SIMULATED] Email to {to} sent. (Configure SMTP_HOST/USER for real sending)";
        }

        try
        {
            using var client = new SmtpClient(smtpHost, int.Parse(smtpPort ?? "587"))
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject,
                Body = content,
                IsBodyHtml = false
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            
            logger.LogInformation("Real email sent to {To}", to);
            return $"Successfully sent real email to {to}.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send real email");
            return $"Failed to send email: {ex.Message}";
        }
    }
}
