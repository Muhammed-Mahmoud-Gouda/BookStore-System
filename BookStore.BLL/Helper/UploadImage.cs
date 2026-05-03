using Microsoft.AspNetCore.Http;

namespace ShopNest.BLL.Helper
{
    public static class UploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public static async Task<string> UploadFileAsync(string folderName, IFormFile file)
        {
            // 1) Validate Extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                throw new Exception("File type not allowed. Only jpg, jpeg, png, webp");

            // 2) Validate Size
            if (file.Length > MaxFileSize)
                throw new Exception("File size exceeds 5MB");

            // 3) Get Folder Path
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", folderName);

            // 4) Create Folder if not exists
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 5) Generate Unique File Name
            string fileName = Guid.NewGuid() + extension;

            // 6) Merge Path with File Name
            string finalPath = Path.Combine(folderPath, fileName);

            // 7) Save File as Stream
            using (var stream = new FileStream(finalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public static bool RemoveFile(string folderName, string fileName)
        {
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                folderName,
                fileName);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
    }
}
