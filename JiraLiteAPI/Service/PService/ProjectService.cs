using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using JiraLiteAPI.Service.PService;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace JiraLiteAPI.Service.PService
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateProject(ProjectDTO dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();

            if (dto.DeadLine < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new Exception("Deadline must be in the future");

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedBy = userId,
                DeadLine = dto.DeadLine,
                Status = dto.Status,
                CreatedOn = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Project created successfully",
                projectId = project.Id
            };
        }

        public async Task<object> GetAllProjects(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();

            if (user.IsInRole("Admin"))
            {
                return await _context.Projects
                    .OrderByDescending(p => p.CreatedOn)
                    .Join(_context.Users, p => p.CreatedBy, u => u.Id, (p, u) => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        CreatedByName = (u.FName ?? "") + " " + (u.LName ?? ""),
                        p.CreatedOn,
                        p.DeadLine,
                        p.Status
                    })
                    .ToListAsync();
            }

            return await _context.ProjectUsers
                .Where(pu => pu.UserId == userId)
                .Select(pu => new
                {
                    pu.Project.Id,
                    pu.Project.Name,
                    pu.Project.Description,
                    pu.Project.CreatedOn,
                    pu.Project.DeadLine,
                    pu.Project.Status
                })
                .ToListAsync();
        }

        public async Task<object> GetProjectById(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();

            if (!user.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId);

                if (!isMember)
                    throw new UnauthorizedAccessException();
            }

            var project = await _context.Projects
                .Join(_context.Users, p => p.CreatedBy, u => u.Id, (p, u) => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CreatedOn,
                    p.DeadLine,
                    CreatedByName = (u.FName ?? "") + " " + (u.LName ?? ""),
                    p.Status
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                throw new Exception("Project not found");

            return project;
        }

        public async Task<string> UpdateProjectStatus(int id, UpdateProjectProgressDTO dto, ClaimsPrincipal user)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                throw new Exception("Project not found");

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!user.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId);

                if (!isMember)
                    throw new UnauthorizedAccessException();
            }

            project.Status = dto.ProjectStatus;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();

            return "Project status updated successfully";
        }

        public async Task<string> UpdateProject(int id, EditProjectDTO dto)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                throw new Exception("Project not found");

            if (dto.DeadLine < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new Exception("Deadline must be in the future");

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.Status = dto.Status;
            project.DeadLine = dto.DeadLine;

            await _context.SaveChangesAsync();

            return "Project updated successfully";
        }

        public async Task<object> DeleteProject(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                throw new Exception("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Project deleted successfully",
                projectId = id
            };
        }
    }
}