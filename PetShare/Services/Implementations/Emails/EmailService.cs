using Microsoft.Extensions.Options;
using PetShare.Configuration;
using PetShare.Services.Interfaces.Emails;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PetShare.Services.Implementations.Emails;

public class EmailService : IEmailService
{
    private readonly IOptions<EmailConfiguration> _config;
    private readonly IWebHostEnvironment _environment;

    public EmailService(IOptions<EmailConfiguration> config, IWebHostEnvironment hostingEnvironment)
    {
        _config = config;
        _environment = hostingEnvironment;
    }

    public async Task SendEmailAsync(string recipientEmail, string recipientUsername, object dynamicTemplateData,
        string templateId)
    {
        if (!_environment.IsProduction() && !_config.Value.SendEmailsInDevMode)
            return;

        var client = new SendGridClient(_config.Value.ApiKey);
        var from = new EmailAddress(_config.Value.SenderEmail, _config.Value.SenderUsername);
        var to = new EmailAddress(recipientEmail, recipientUsername);
        var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
        await client.SendEmailAsync(msg);
    }

    public async Task SendStatusUpdateEmail(string recipientEmail, string recipientUsername, string newStatus)
    {
        var dynamicTemplateData = new
        {
            username = recipientUsername,
            status = newStatus
        };

        await SendEmailAsync(recipientEmail, recipientUsername, dynamicTemplateData, _config.Value.TemplateId);
    }
}
