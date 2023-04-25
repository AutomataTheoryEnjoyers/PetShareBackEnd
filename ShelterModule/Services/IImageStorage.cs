namespace ShelterModule.Services;

public interface IImageStorage
{
    Task<string> UploadImageAsync(IFormFile image);
}
