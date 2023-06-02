namespace PetShare.Services.Interfaces.Emails;

public interface IEmailService
{
    public Task SendEmailAsync(string recipientEmail, string recipientUsername, object dynamicTemplateData,
        string templateId);

    public Task SendStatusUpdateEmail(string recipientEmail, string recipientUsername, string newStatus);
}
