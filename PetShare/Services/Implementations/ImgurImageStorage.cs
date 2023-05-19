using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Options;
using PetShare.Configuration;
using PetShare.Results;
using PetShare.Services.Interfaces;

namespace PetShare.Services.Implementations;

public sealed class ImgurImageStorage : IImageStorage
{
    private readonly IOptions<ImgurConfiguration> _config;

    public ImgurImageStorage(IOptions<ImgurConfiguration> config)
    {
        _config = config;
    }

    public async Task<Result<string>> UploadImageAsync(IFormFile imageFile)
    {
        int? GetCode(dynamic error)
        {
            try
            {
                return (int)error.code;
            }
            catch (RuntimeBinderException)
            {
                return null;
            }
        }

        var response = await _config.Value.UploadUrl.WithHeader("Authorization", $"Client-ID {_config.Value.ApiKey}").
                                     AllowAnyHttpStatus().
                                     PostMultipartAsync(mp => mp.AddFile("image", imageFile.OpenReadStream(),
                                                                         imageFile.FileName, imageFile.ContentType));

        if (response.StatusCode >= 500)
            return new UploadError(response.StatusCode);

        var data = await response.GetJsonAsync();
        if (data is null)
            return new WrongFormat();

        try
        {
            if (data.success)
                return data.data.link;

            return data.data.error switch
            {
                string e when e.StartsWith("Invalid URL") => new InvalidOperation("File type is invalid"),
                "File is over the size limit" => new InvalidOperation("Image is too large"),
                "No image data was sent to the upload api" => new InvalidOperation("Form field is not named 'image'"),
                var e when GetCode(e) == 1003 => new InvalidOperation("File type is invalid"),
                _ => new UploadError(response.StatusCode)
            };
        }
        catch (RuntimeBinderException)
        {
            return new WrongFormat();
        }
    }

    private sealed record UploadError(int StatusCode) : ResultState
    {
        public override ActionResult ToActionResult()
        {
            return new ObjectResult(new { error = $"Imgur upload failed: got {StatusCode}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private sealed record WrongFormat : ResultState
    {
        public override ActionResult ToActionResult()
        {
            return new ObjectResult(new { error = "Imgur upload failed: response has invalid format" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
