using Flurl.Http;
using Microsoft.Extensions.Options;
using ShelterModule.Configuration;

namespace ShelterModule.Services.Implementations;

public sealed class ImgurImageStorage : IImageStorage
{
    private readonly IOptions<ImgurConfiguration> _config;

    public ImgurImageStorage(IOptions<ImgurConfiguration> config)
    {
        _config = config;
    }

    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        using var memoryStream = new MemoryStream();
        await imageFile.CopyToAsync(memoryStream);

        var response = await _config.Value.UploadUrl.WithHeader("Authorization", $"Client-ID {_config.Value.ApiKey}").
                                     PostAsync(new ByteArrayContent(memoryStream.ToArray()));

        /*
         * TODO: Handle user errors (once applications are merged, so that Result can be used):
         *      Invalid file type => data.error.code: 1003
         *      File too big => data.error: "File is over the size limit"
         */

        if (response.StatusCode != StatusCodes.Status200OK)
            throw new InvalidOperationException("Error uploading image");

        var data = await response.GetJsonAsync();
        return data.data.link;
    }
}
