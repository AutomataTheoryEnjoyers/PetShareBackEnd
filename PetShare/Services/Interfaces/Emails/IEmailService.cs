namespace PetShare.Services.Interfaces.Emails;

public interface IEmailService
{
    Task SendEmailAsync(string recipientEmail, string recipientUsername, object dynamicTemplateData,
        string templateId);

    Task SendStatusUpdateEmail(string recipientEmail, string recipientUsername, string newStatus);
}
