using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using ShelterModule.Configuration;
using ShelterModule.Results;
using ShelterModule.Services.Implementations;
using Xunit;

namespace ShelterModuleTests;

[Trait("Category", "Unit")]
public sealed class ImgurImageStorageTests
{
    private readonly ImgurImageStorage _storage;

    public ImgurImageStorageTests()
    {
        var config = Substitute.For<IOptions<ImgurConfiguration>>();
        config.Value.Returns(new ImgurConfiguration
        {
            UploadUrl = "https://api.imgur.com/3/image",
            ApiKey = "d0161263c2653d4"
        });
        _storage = new ImgurImageStorage(config);
    }

    [Fact]
    public async Task UploadImageShouldUploadImageAndReturnLink()
    {
        const string path = "Files/small-image.png";
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var result = await _storage.UploadImageAsync(new FormFile(stream, 0, stream.Length, "image", "photo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        });

        result.HasValue.Should().BeTrue();

        var bytes = await result.Value.GetBytesAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UploadImageShouldFailIfFileHasWrongType()
    {
        const string path = "Files/not-an-image.txt";
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var result = await _storage.UploadImageAsync(new FormFile(stream, 0, stream.Length, "image", "photo.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        });

        result.HasValue.Should().BeFalse();
        result.State.Should().BeOfType<InvalidOperation>();
    }
}
