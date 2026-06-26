using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SchoolMgmt.Services.Interfaces;
using SchoolMgmt.Settings;

namespace SchoolMgmt.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SendGridSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody)
    {
        try
        {
            var client = new SendGridClient(_settings.ApiKey);
            var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(
                from, to, subject, null, htmlBody);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning(
                    "SendGrid failed for {Email}: {Status}",
                    toEmail, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed for {Email}", toEmail);
        }
    }

    public async Task SendBulkAsync(
        IEnumerable<(string Email, string Name)> recipients,
        string subject,
        string htmlBody)
    {
        var tasks = recipients.Select(r =>
            SendAsync(r.Email, r.Name, subject, htmlBody));
        await Task.WhenAll(tasks);
    }
}