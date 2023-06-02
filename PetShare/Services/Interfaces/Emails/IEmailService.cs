using SendGrid.Helpers.Mail;
using SendGrid;

namespace PetShare.Services.Interfaces.Emails
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string recpientEmail, string recipientUsername, object dynamicTemplateData, string tempalteId);
        public Task SendStatusUpdateEmail(string recpientEmail, string recipientUsername, string newStatus);
    }
}
