namespace ShelterModule.Configuration;

public sealed class ImgurConfiguration
{
    public const string SectionName = "Imgur";

    public required string UploadUrl { get; init; }
    public required string ApiKey { get; init; }
}
