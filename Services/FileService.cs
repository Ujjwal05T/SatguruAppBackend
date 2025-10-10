namespace WastageUploadService.Services;

public interface IFileService
{
    Task<List<string>> SaveImagesAsync(List<IFormFile> images, string inwardChallanId);
    Task DeleteImagesAsync(List<string> imageUrls);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private const string UploadsFolder = "uploads/wastage";

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<List<string>> SaveImagesAsync(List<IFormFile> images, string inwardChallanId)
    {
        var imageUrls = new List<string>();

        if (images == null || !images.Any())
            return imageUrls;

        // Create upload directory if it doesn't exist
        var uploadPath = Path.Combine(_environment.WebRootPath, UploadsFolder, inwardChallanId);
        Directory.CreateDirectory(uploadPath);

        foreach (var image in images)
        {
            try
            {
                // Validate file
                if (image.Length == 0)
                    continue;

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning($"Invalid file type: {extension}");
                    continue;
                }

                // Validate file size (10MB max)
                if (image.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning($"File too large: {image.Length} bytes");
                    continue;
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Store relative URL
                var relativeUrl = $"/{UploadsFolder}/{inwardChallanId}/{fileName}";
                imageUrls.Add(relativeUrl);

                _logger.LogInformation($"Image saved: {relativeUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving image: {image.FileName}");
            }
        }

        return imageUrls;
    }

    public async Task DeleteImagesAsync(List<string> imageUrls)
    {
        if (imageUrls == null || !imageUrls.Any())
            return;

        foreach (var url in imageUrls)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, url.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation($"Image deleted: {url}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {url}");
            }
        }
    }
}
