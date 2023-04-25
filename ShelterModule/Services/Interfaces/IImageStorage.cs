using ShelterModule.Results;

namespace ShelterModule.Services.Interfaces;

public interface IImageStorage
{
    Task<Result<string>> UploadImageAsync(IFormFile image);
}
