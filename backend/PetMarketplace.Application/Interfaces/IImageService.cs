namespace PetMarketplace.Application.Interfaces;

public interface IImageService
{
    Task<(string ImageUrl, string PublicId)> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
