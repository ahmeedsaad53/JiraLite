using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
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


        public async Task<ServiceResponse<ProjectResponseDTO>> CreateProject(ProjectDTO dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<ProjectResponseDTO>.Fail("Unauthorized");

            if (dto.DeadLine < DateOnly.FromDateTime(DateTime.UtcNow))
                return ServiceResponse<ProjectResponseDTO>.Fail("Deadline must be in the future");

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

            return ServiceResponse<ProjectResponseDTO>.SuccessResponse(
                new ProjectResponseDTO
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    DeadLine = project.DeadLine,
                    Status = project.Status,
                    CreatedOn = project.CreatedOn,
                },
                "Project created successfully"
            );
        }

        public async Task<ServiceResponse<IEnumerable<ProjectResponseDTO>>> GetAllProjects(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<IEnumerable<ProjectResponseDTO>>.Fail("Unauthorized");

            List<ProjectResponseDTO> result;

            if (user.IsInRole("Admin"))
            {
                result = await _context.Projects
                    .Join(_context.Users,
                        p => p.CreatedBy,
                        u => u.Id,
                        (p, u) => new ProjectResponseDTO
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            CreatedOn = p.CreatedOn,
                            DeadLine = p.DeadLine,
                            Status = p.Status,
                            CreatedByName = (u.FName ?? "") + " " + (u.LName ?? "")
                        })
                    .OrderByDescending(p => p.CreatedOn)
                    .ToListAsync();
            }
            else
            {
                result = await _context.ProjectUsers
                    .Where(pu => pu.UserId == userId)
                    .SelectMany(pu => _context.Projects
                        .Where(p => p.Id == pu.ProjectId)
                        .Join(_context.Users,
                            p => p.CreatedBy,
                            u => u.Id,
                            (p, u) => new ProjectResponseDTO
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Description = p.Description,
                                CreatedOn = p.CreatedOn,
                                DeadLine = p.DeadLine,
                                Status = p.Status,
                                CreatedByName = (u.FName ?? "") + " " + (u.LName ?? "")
                            }))
                    .OrderByDescending(p => p.CreatedOn)
                    .ToListAsync();
            }

            return ServiceResponse<IEnumerable<ProjectResponseDTO>>
                .SuccessResponse(result, "Projects retrieved");
        }

        public async Task<ServiceResponse<ProjectResponseDTO>> GetProjectById(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<ProjectResponseDTO>.Fail("Unauthorized");

            if (!user.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == id && p.UserId == userId);

                if (!isMember)
                    return ServiceResponse<ProjectResponseDTO>.Fail("Forbidden");
            }

            var project = await _context.Projects
                .Join(_context.Users,
                    p => p.CreatedBy,
                    u => u.Id,
                    (p, u) => new ProjectResponseDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CreatedOn = p.CreatedOn,
                        DeadLine = p.DeadLine,
                        Status = p.Status,
                        CreatedByName = (u.FName ?? "") + " " + (u.LName ?? "")
                    })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return ServiceResponse<ProjectResponseDTO>.Fail("Project not found");

            return ServiceResponse<ProjectResponseDTO>.SuccessResponse(project);
        }

        public async Task<ServiceResponse<string>> UpdateProject(int id, EditProjectDTO dto)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return ServiceResponse<string>.Fail("Project not found");

            if (dto.DeadLine < DateOnly.FromDateTime(DateTime.UtcNow))
                return ServiceResponse<string>.Fail("Deadline must be in the future");

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.Status = dto.Status;
            project.DeadLine = dto.DeadLine;

            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Updated", "Project updated successfully");
        }

        public async Task<ServiceResponse<string>> UpdateProjectStatus(int id, UpdateProjectProgressDTO dto, ClaimsPrincipal user)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return ServiceResponse<string>.Fail("Project not found");

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!user.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == id && p.UserId == userId);

                if (!isMember)
                    return ServiceResponse<string>.Fail("Forbidden");
            }

            project.Status = dto.ProjectStatus;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Updated", "Status updated successfully");
        }

        public async Task<ServiceResponse<string>> DeleteProject(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return ServiceResponse<string>.Fail("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Deleted", "Project deleted successfully");
        }
    }
}