using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class EmailPlugin(ILogger<EmailPlugin> logger)
{
    [KernelFunction("send_email")]
    [Description("Sends an email with a subject and content")]
    public string SendEmail(
        [Description("Target email address")] string to,
        [Description("Email subject")] string subject,
        [Description("Email body content")] string content)
    {
        logger.LogInformation("EmailPlugin.send_email called to={To}, subject={Subject}", to, subject);
        return $"Email sent to {to} with subject '{subject}' and content: {content}";
    }
}
