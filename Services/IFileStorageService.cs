namespace ProductManagement.Web.Services
{
    public interface IFileStorageService
    {
        string? ValidateImage(IFormFile? file);
        Task<string> UploadProductImageAsync(IFormFile file, string? customPrefix = null);
        void DeleteFile(string? relativeFilePath);
    }
}
