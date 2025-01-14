public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadsPath = "uploads/products";

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_environment.WebRootPath, _uploadsPath, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{_uploadsPath}/{fileName}";
    }
}