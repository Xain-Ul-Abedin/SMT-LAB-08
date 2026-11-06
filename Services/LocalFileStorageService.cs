namespace ProductManagement.Web.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSizeBytes = 8 * 1024 * 1024; // 8MB limit
        private static readonly HashSet<string> PermittedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".svg"
        };

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string ResolveTargetFolder()
        {
            string root = !string.IsNullOrWhiteSpace(_env.WebRootPath)
                ? _env.WebRootPath
                : Path.Combine(_env.ContentRootPath, "wwwroot");

            string target = Path.Combine(root, "uploads", "catalog");
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
            return target;
        }

        public string? ValidateImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return "Please choose an image file for this product.";
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return "The selected file exceeds the 8 MB upload threshold.";
            }

            string ext = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
            if (!PermittedExtensions.Contains(ext))
            {
                return $"Unsupported format. Permitted extensions: {string.Join(", ", PermittedExtensions)}";
            }

            return null;
        }

        public async Task<string> UploadProductImageAsync(IFormFile file, string? customPrefix = null)
        {
            string folder = ResolveTargetFolder();
            string ext = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
            string safePrefix = !string.IsNullOrWhiteSpace(customPrefix)
                ? $"{customPrefix.Trim().ToLowerInvariant()}-"
                : "prd-";

            string fileName = $"{safePrefix}{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N[..8]}{ext}";
            string fullDiskPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullDiskPath, FileMode.CreateNew, FileAccess.Write);
            await file.CopyToAsync(stream);

            return $"uploads/catalog/{fileName}";
        }

        public void DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath)) return;

            string normalized = relativeFilePath.Replace('/', Path.DirectorySeparatorChar);
            string root = !string.IsNullOrWhiteSpace(_env.WebRootPath)
                ? _env.WebRootPath
                : Path.Combine(_env.ContentRootPath, "wwwroot");

            string fullDiskPath = Path.GetFullPath(Path.Combine(root, normalized));

            if (!fullDiskPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return; // Guard against directory traversal
            }

            if (File.Exists(fullDiskPath))
            {
                File.Delete(fullDiskPath);
            }
        }
    }
}
