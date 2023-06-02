namespace PetShare.Configuration
{
    public class EmailConfiguration
    {
        public const string SectionName = "Email";

        public required string APIKey { get; init; }
        public required string TemplateId { get; init; }
        public required string SenderEmail { get; init; }
        public required string SenderUsername { get; init; }
        public required bool SendEmailsInDevMode { get; init; }
    }
}
