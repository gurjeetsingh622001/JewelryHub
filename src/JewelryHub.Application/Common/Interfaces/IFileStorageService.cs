namespace JewelryHub.Application.Common.Interfaces;

/// <summary>
/// Abstracts where an uploaded file actually lands. Currently backed by
/// local disk under the API's wwwroot/uploads (see
/// Infrastructure/Storage/LocalFileStorageService) — swappable for cloud
/// blob storage later without any Application-layer handler needing to
/// change, same reasoning as every other Infrastructure boundary here.
/// </summary>
public interface IFileStorageService
{
    /// <returns>A relative URL path (e.g. "/uploads/products/{guid}.jpg") the client can use directly as an &lt;img&gt; src or store as ProductImage.Url / SellerDocument.FileUrl.</returns>
    Task<string> SaveAsync(byte[] content, string originalFileName, string folder, CancellationToken cancellationToken = default);
}
