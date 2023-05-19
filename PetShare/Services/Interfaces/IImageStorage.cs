using PetShare.Results;

namespace PetShare.Services.Interfaces;

public interface IImageStorage
{
    Task<Result<string>> UploadImageAsync(IFormFile image);
}
