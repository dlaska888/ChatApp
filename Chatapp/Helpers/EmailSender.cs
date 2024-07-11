using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebService.Models.Options;

namespace WebService.Helpers;

public class EmailSender(
    IOptions<AuthMessageSenderOptions> optionsAccessor,
    ILogger<EmailSender> logger)
    : IEmailSender
{
    private AuthMessageSenderOptions Options { get; } = optionsAccessor.Value; //Set with Secret Manager.

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            throw new Exception("Null SendGridKey");
        }

        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress("dawidlaska888@gmail.com", "ChatApp no-reply"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        logger.LogInformation(response.IsSuccessStatusCode
            ? $"Email to {toEmail} queued successfully!"
            : $"Failure Email to {toEmail}");
    }
}