namespace SchoolMgmt.Services.Interfaces;

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody);

    Task SendBulkAsync(
        IEnumerable<(string Email, string Name)> recipients,
        string subject,
        string htmlBody);
}