using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace DevContextNexus.API.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder, string? publicId = null);
        Task<string> UploadContentAsync(string content, string fileName, string folder);
        Task<string> GetFileUrlAsync(string publicId);
        Task DeleteFileAsync(string publicId);
        Task DeleteFolderAsync(string folder);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudinaryUrl = configuration["Cloudinary:Url"];
            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                // Fallback to environment variable if not in appsettings
                cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
            }

            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                throw new Exception("Cloudinary URL not found in configuration.");
            }

            _cloudinary = new Cloudinary(cloudinaryUrl);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder, string? publicId = null)
        {
            if (file.Length == 0) return string.Empty;

            using var stream = file.OpenReadStream();
            
            var extension = Path.GetExtension(file.FileName).ToLower();
            var isImage = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" }.Contains(extension);
            
            // Logic Change: Append subdirectory to 'Folder' parameter, keep PublicId clean.
            // 'publicId' argument might be "docs/readme". We want Folder="Project/docs", PublicId="readme".
            
            var targetFolder = folder;
            var targetPublicId = publicId ?? Path.GetFileNameWithoutExtension(file.FileName);

            if (!string.IsNullOrEmpty(publicId))
            {
                var directory = Path.GetDirectoryName(publicId);
                if (!string.IsNullOrEmpty(directory))
                {
                    // Append subdirectory to the root folder
                    targetFolder = $"{folder}/{directory.Replace("\\", "/")}";
                    targetPublicId = Path.GetFileName(publicId); // Keep extension if it was in publicId, or just name
                }
            }
            
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = targetFolder,
                PublicId = targetPublicId,
                Overwrite = true,
                UseFilename = true, 
                UniqueFilename = false
            };

            if (isImage)
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = targetFolder,
                    PublicId = targetPublicId,
                    Overwrite = true,
                    UseFilename = true,
                    UniqueFilename = false
                };
                var result = await _cloudinary.UploadAsync(imageParams);
                return result.SecureUrl.ToString();
            }
            else 
            {
                var result = await _cloudinary.UploadAsync(uploadParams);
                return result.SecureUrl.ToString();
            }
        }

        public async Task<string> UploadContentAsync(string content, string fileName, string folder)
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            
            // Logic Change for Content:
            // fileName = "docs/readme.md"
            // Folder = "ProjectName"
            // We want Folder = "ProjectName/docs", PublicId = "readme"
            
            var directory = Path.GetDirectoryName(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            
            var targetFolder = folder;
            if (!string.IsNullOrEmpty(directory))
            {
                targetFolder = $"{folder}/{directory.Replace("\\", "/")}";
            }

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = targetFolder,
                PublicId = nameWithoutExt,
                Overwrite = true,
                UseFilename = true,
                UniqueFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl.ToString();
        }

        public Task<string> GetFileUrlAsync(string publicId)
        {
            return Task.FromResult(_cloudinary.Api.Url.Secure(true).BuildUrl(publicId));
        }

        public async Task DeleteFileAsync(string publicId)
        {
            // Try deleting as Raw first (expects full filename usually)
            var deletionParamsRaw = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
            var resultRaw = await _cloudinary.DestroyAsync(deletionParamsRaw);
            
            // If Raw deletion didn't find it (or even if it did, to be safe if ID is ambiguous), try Image.
            // For Images, Cloudinary expects PublicId WITHOUT extension usually.
            // If 'publicId' has extension (e.g. "folder/img.png"), strip it for Image deletion.
            
            string publicIdNoExt = publicId;
            if (Path.HasExtension(publicId))
            {
                publicIdNoExt = publicId.Substring(0, publicId.LastIndexOf('.'));
            }

            var deletionParamsImage = new DeletionParams(publicIdNoExt) { ResourceType = ResourceType.Image };
            await _cloudinary.DestroyAsync(deletionParamsImage);
            
            // Also try Image WITH extension just in case (some configs allow it)
            if (publicId != publicIdNoExt)
            {
                 var deletionParamsImageWithExt = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                 await _cloudinary.DestroyAsync(deletionParamsImageWithExt);
            }
        }

        public async Task DeleteFolderAsync(string folder)
        {
             // Cloudinary requires folder to be empty before deleting.
             // We assume caller has deleted files. ContextController loops through files DB to delete them.
             // But to be extra safe, we could delete resources by prefix.
             // However, let's trust the logic for now and just try to delete the folder.
             // Note: DeleteFolder API might fail if not empty.
             
             await _cloudinary.DeleteFolderAsync(folder);
        }
    }
}
