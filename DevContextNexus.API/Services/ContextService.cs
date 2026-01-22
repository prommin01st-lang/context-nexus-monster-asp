using DevContextNexus.API.Data;
using DevContextNexus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DevContextNexus.API.Services
{
    public interface IContextService
    {
        Task<string> GetContextContentAsync(string projectName, string filePath);
        Task<ContextFile> SyncContextFileAsync(string projectName, string filePath);
        Task<Project> CreateProjectAsync(string name);
        Task<List<Project>> GetAllProjectsAsync();
        Task<List<ContextFile>> GetProjectFilesAsync(string projectName);
        Task<ContextFile> SaveContextContentAsync(string projectName, string filePath, string content);
        Task DeleteContextFileAsync(string projectName, string filePath);
        
        Task DeleteProjectAsync(string projectName);
        Task<SystemOverview> GetSystemOverviewAsync();
    }

    public class SystemOverview
    {
        public int TotalProjects { get; set; }
        public int TotalFiles { get; set; }
        public List<ProjectSummary> Projects { get; set; } = new();
    }

    public class ProjectSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FileCount { get; set; }
    }

    public class ContextService : IContextService
    {
        private readonly AppDbContext _context;

        public ContextService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SystemOverview> GetSystemOverviewAsync()
        {
            var projectCount = await _context.Projects.CountAsync();
            var fileCount = await _context.ContextFiles.CountAsync();
            
            var projects = await _context.Projects
                .Select(p => new ProjectSummary 
                { 
                    Id = p.Id, 
                    Name = p.Name,
                    FileCount = _context.ContextFiles.Count(cf => cf.ProjectId == p.Id)
                })
                .ToListAsync();

            return new SystemOverview
            {
                TotalProjects = projectCount,
                TotalFiles = fileCount,
                Projects = projects
            };
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<List<ContextFile>> GetProjectFilesAsync(string projectName)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null) throw new KeyNotFoundException($"Project '{projectName}' not found.");

            return await _context.ContextFiles
                .Where(cf => cf.ProjectId == project.Id)
                .ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(string name)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == name);
            if (project == null)
            {
                project = new Project { Id = Guid.NewGuid(), Name = name };
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
            }
            return project;
        }

        public async Task<string> GetContextContentAsync(string projectName, string filePath)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project '{projectName}' not found.");
            }

            var contextFile = await _context.ContextFiles
                .FirstOrDefaultAsync(cf => cf.ProjectId == project.Id && cf.FilePath == filePath);

            if (contextFile == null)
            {
                 throw new KeyNotFoundException($"File '{filePath}' not found.");
            }

            // Return Public URL if available.
            return contextFile.PublicUrl ?? string.Empty;
        }

        // Deprecated/Modified for Metadata Sync
        public async Task<ContextFile> SyncContextFileAsync(string projectName, string filePath)
        {
            // Just ensure tracking exists? Or no-op?
            // With Cloudinary push model, sync is less relevant unless we have listing capability.
            // For now, let's just make sure record exists if called.
             var project = await CreateProjectAsync(projectName);
             
             var contextFile = await _context.ContextFiles
                .FirstOrDefaultAsync(cf => cf.ProjectId == project.Id && cf.FilePath == filePath);

             if (contextFile == null)
             {
                 contextFile = new ContextFile
                 {
                     Id = Guid.NewGuid(),
                     ProjectId = project.Id,
                     FilePath = filePath,
                     LastSha = "CLOUDINARY" // Placeholder
                 };
                 _context.ContextFiles.Add(contextFile);
                 await _context.SaveChangesAsync();
             }
             return contextFile;
        }

        public async Task<ContextFile> SaveContextContentAsync(string projectName, string filePath, string contentOrUrl)
        {
            var project = await CreateProjectAsync(projectName);

            var contextFile = await _context.ContextFiles
                .FirstOrDefaultAsync(cf => cf.ProjectId == project.Id && cf.FilePath == filePath);

            if (contextFile == null)
            {
                contextFile = new ContextFile
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    FilePath = filePath,
                    PublicUrl = contentOrUrl, // Storing URL here
                    LastSha = "CLOUDINARY"
                };
                _context.ContextFiles.Add(contextFile);
            }
            else
            {
                contextFile.PublicUrl = contentOrUrl;
                contextFile.LastSha = "CLOUDINARY_UPDATED"; // Update placeholder
                _context.ContextFiles.Update(contextFile);
            }

            await _context.SaveChangesAsync();
            return contextFile;
        }

        public async Task DeleteContextFileAsync(string projectName, string filePath)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null) throw new KeyNotFoundException($"Project '{projectName}' not found.");

            var contextFile = await _context.ContextFiles
                .FirstOrDefaultAsync(cf => cf.ProjectId == project.Id && cf.FilePath == filePath);

            if (contextFile != null)
            {
                _context.ContextFiles.Remove(contextFile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProjectAsync(string projectName)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null) throw new KeyNotFoundException($"Project '{projectName}' not found.");

            // Cascading delete is handled by DB FK usually, but let's be explicit if needed
            // But for now, we rely on EF Core or manual cleanup if no Cascade.
            // Let's manually remove files to be safe with EF tracking.
            var files = _context.ContextFiles.Where(f => f.ProjectId == project.Id);
            _context.ContextFiles.RemoveRange(files);
            
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }
}
