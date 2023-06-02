using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using PetShare.Services.Interfaces.Emails;
using PetShare.Configuration;
using Azure.Core;
using Flurl;
using Microsoft.AspNetCore.Identity;

namespace PetShare.Services.Implementations.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<EmailConfiguration> _config;
        private readonly IWebHostEnvironment _environment;

        public EmailService(IOptions<EmailConfiguration> config, IWebHostEnvironment hostingEnvironment)
        {
            _config = config;
            _environment = hostingEnvironment;
        }

        public async Task SendEmailAsync(string recpientEmail, string recipientUsername, object dynamicTemplateData, string templateId)
        {
            if (!_environment.IsProduction() && !_config.Value.SendEmailsInDevMode)
                return;

            var client = new SendGridClient(_config.Value.APIKey);
            var from = new EmailAddress(_config.Value.SenderEmail, _config.Value.SenderUsername);
            var to = new EmailAddress(recpientEmail, recipientUsername);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
            var response = await client.SendEmailAsync(msg);
        }

        public async Task SendStatusUpdateEmail(string recpientEmail, string recipientUsername, string newStatus)
        {
            var dynamicTemplateData = new
            {
                username = recipientUsername,
                status = newStatus
            };

            await SendEmailAsync(recpientEmail, recipientUsername, dynamicTemplateData, _config.Value.TemplateId);
        }
    }
}