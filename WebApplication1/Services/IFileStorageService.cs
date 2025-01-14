using Microsoft.AspNetCore.Http;

namespace WebApplication1.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file);
    }
}