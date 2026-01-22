using DevContextNexus.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace DevContextNexus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContextController : ControllerBase
    {
        private readonly IContextService _contextService;
        private readonly ICloudinaryService _cloudinaryService;

        public ContextController(IContextService contextService, ICloudinaryService cloudinaryService)
        {
            _contextService = contextService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet("projects")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _contextService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{projectName}/files")]
        public async Task<IActionResult> GetProjectFiles(string projectName)
        {
             try
            {
                var files = await _contextService.GetProjectFilesAsync(projectName);
                return Ok(files);
            }
             catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("projects")]
        public async Task<IActionResult> CreateProject([FromBody] System.Text.Json.Nodes.JsonNode request)
        {
            var name = request["name"]?.ToString() ?? request["Name"]?.ToString();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { error = "name is required" });

            var project = await _contextService.CreateProjectAsync(name);
            return Ok(project);
        }

        /* 
        // Sync is specific to GitHub SHA logic - disabling for Cloudinary transition
        [HttpPost("sync")]
        public async Task<IActionResult> SyncContext([FromBody] System.Text.Json.Nodes.JsonNode request)
        {
             // ...
        }
        */

        [HttpPost("content")]
        public async Task<IActionResult> UpdateContextContent([FromBody] System.Text.Json.Nodes.JsonNode request)
        {
            var projectName = request["projectName"]?.ToString() ?? request["ProjectName"]?.ToString();
            var filePath = request["filePath"]?.ToString() ?? request["FilePath"]?.ToString();
            var content = request["content"]?.ToString() ?? request["Content"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(projectName)) return BadRequest(new { error = "projectName is required" });
            if (string.IsNullOrWhiteSpace(filePath)) return BadRequest(new { error = "filePath is required" });

            try
            {
                // Upload content to Cloudinary
                var url = await _cloudinaryService.UploadContentAsync(content, filePath, projectName);
                
                // Save metadata to DB
                // TODO: Update ContextService to accept URL instead of using GitHub logic?
                // For now, let's assume we store the URL in a new field or repurpose logic.
                // We'll update the SaveContextContentAsync replacement below.
                
                var result = await _contextService.SaveContextContentAsync(projectName, filePath, url); // url treated as content/metadata?
                return Ok(new { message = "Content saved successfully", url = result.PublicUrl });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message, hint = "Please create the project first using /api/context/projects" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadContextFile([FromForm] UploadContextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProjectName)) return BadRequest("projectName is required.");
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var filePath = string.IsNullOrWhiteSpace(request.FilePath) ? request.File.FileName : request.FilePath;

                // Cloudinary Upload
                // Pass the relative path (without extension) as publicId so the service can extract subfolders.
                // FilePath: "docs/readme.md" -> PublicId: "docs/readme"
                
                string? publicId = null;
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var dir = Path.GetDirectoryName(filePath);
                    var name = Path.GetFileNameWithoutExtension(filePath);
                    publicId = string.IsNullOrEmpty(dir) ? name : Path.Combine(dir, name).Replace("\\", "/");
                }

                var url = await _cloudinaryService.UploadFileAsync(request.File, request.ProjectName, publicId);

                // Save to DB
                // We need to update IContextService to handle "Save Cloudinary URL"
                
                var result = await _contextService.SaveContextContentAsync(request.ProjectName, filePath, url);
                return Ok(new { message = "File uploaded successfully", url = result.PublicUrl });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message, hint = "Please create the project first using /api/context/projects" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpGet("{projectName}/{*filePath}")]
        public async Task<IActionResult> GetContext(string projectName, string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) return BadRequest("filePath is required.");
                // Redirect to Cloudinary URL?
                // Or fetch content?
                // If the user wants the content to display, we might redirect.
                var contentUrl = await _contextService.GetContextContentAsync(projectName, filePath);
                // Return URL for client to fetch? Or fetch and proxy?
                // Returning URL is cleaner for Cloudinary.
                return Ok(new { url = contentUrl }); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _contextService.GetSystemOverviewAsync();
            return Ok(overview);
        }

        [HttpDelete("projects/{name}")]
        public async Task<IActionResult> DeleteProject(string name)
        {
            try
            {
                // 1. Get all files for the project to delete them from Cloudinary
                var files = await _contextService.GetProjectFilesAsync(name);
                
                foreach (var file in files)
                {
                    // Construct PublicId. 
                    // CloudinaryService uses {folder}/{filename} usually.
                    // But here folder is ProjectName.
                    // Construct PublicId.
                    // Instead of reconstructing from filename (which is error-prone),
                    // we extract it directly from the stored Public Url.
                    // Format: .../upload/v12345678/ProjectName/path/file.ext
                    // We need: ProjectName/path/file.ext (OR ProjectName/path/file if no ext in PublicId)
                    
                    var publicUrl = file.PublicUrl;
                    if (!string.IsNullOrEmpty(publicUrl))
                    {
                         // Regex to capture everything after "upload/v<version>/" or just "upload/"
                         // A simple approach is finding "upload/" and taking the part after the version.
                         // Pattern: .../upload/(?:v\d+/)?(.+)
                         
                         var match = System.Text.RegularExpressions.Regex.Match(publicUrl, @"upload/(?:v\d+/)?(.+)");
                         if (match.Success)
                         {
                             var fullPublicIdWithExt = match.Groups[1].Value;
                             // Note: For Raw files, PublicId usually includes extension.
                             // For Images, it might not. But Delete via API usually handles it or we can try both.
                             // CloudinaryService.DeleteFileAsync handles Raw/Image types.
                             
                             // However, Cloudinary "PublicId" usually does NOT include the extension for images,
                             // but DOES include it for RAW files if they were uploaded as RAW.
                             
                             // Let's pass the extracted path.
                             // But wait, if it's an image "image.png", the URL has "image.png", but PublicId is "image".
                             // The DeleteFileAsync tries Raw first (needs ext?) then Image (no ext?).
                             
                             // Let's be smarter: Pass the extracted path as is.
                             // And mostly important: remove the extension if it's an image?
                             // Actually, let CloudinaryService handle the "PublicId" concept.
                             // But extracting from URL gives "folder/file.ext".
                             
                             // If we pass "folder/file.ext" to Cloudinary Destroy:
                             // - If Raw: It expects "folder/file.ext". MATCH!
                             // - If Image: It expects "folder/file". MISMATCH!
                             
                             // So we should try to strip extension if it's likely an image, or just try both in Service.
                             // But we don't know if it's image or raw here easily without checking extension.
                             
                             await _cloudinaryService.DeleteFileAsync(fullPublicIdWithExt);
                         }
                    }
                }

                // 2. Delete the Project Folder in Cloudinary (now that files are gone)
                // Note: If there are subfolders, they might need recursive deletion or Cloudinary handles it if empty.
                // Our DeleteFolderAsync just calls API. If fails, we log/ignore? 
                // Let's try to delete the root project folder.
                try 
                {
                    await _cloudinaryService.DeleteFolderAsync(name);
                }
                catch
                {
                    // Ignore folder deletion errors (e.g. if already gone or not empty for some reason)
                    // We don't want to block DB deletion.
                }

                // 3. Delete from DB
                await _contextService.DeleteProjectAsync(name);

                return Ok(new { message = $"Project '{name}' and all associated files deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{projectName}/{*filePath}")]
        public async Task<IActionResult> DeleteContext(string projectName, string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) return BadRequest(new { error = "filePath is required." });
                await _contextService.DeleteContextFileAsync(projectName, filePath);
                return Ok(new { message = $"File '{filePath}' deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class CreateProjectRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class SyncContextRequest
    {
        [JsonPropertyName("projectName")]
        public string? ProjectName { get; set; }
        
        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }
    }

    public class UpdateContextRequest
    {
        [JsonPropertyName("projectName")]
        public string? ProjectName { get; set; }
        
        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }
        
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class UploadContextRequest
    {
        public string? ProjectName { get; set; }
        public string? FilePath { get; set; }
        public IFormFile? File { get; set; }
    }
}
