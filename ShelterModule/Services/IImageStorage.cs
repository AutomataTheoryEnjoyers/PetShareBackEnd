using ShelterModule.Results;

namespace ShelterModule.Services;

public interface IImageStorage
{
    Task<Result<string>> UploadImageAsync(IFormFile image);
}
