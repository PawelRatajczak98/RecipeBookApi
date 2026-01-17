using Application.Interfaces;
using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;
        private readonly string _baseUrl;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        public FileStorageService(IConfiguration configuration)
        {
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads/recipes";
            _baseUrl = configuration["FileStorage:BaseUrl"] ?? "https://localhost:7090";
            _maxFileSize = long.Parse(configuration["FileStorage:MaxFileSizeMB"] ?? "5") * 1024 * 1024;
            _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            Directory.CreateDirectory(_uploadPath);
        }

        public bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                throw new ValidationException($"Plik przekracza limit {_maxFileSize / 1024 / 1024}MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ValidationException("Nieprawidłowy typ pliku. Dozwolone: jpg, jpeg, png, gif, webp");

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new ValidationException("Nieprawidłowy content type");

            return true;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (!ValidateImageFile(file))
                throw new ValidationException("Nieprawidłowy plik");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var folderPath = Path.Combine(_uploadPath, folder);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_uploadPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }

        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            return $"{_baseUrl}/uploads/recipes/{filePath}";
        }
    }
}
