using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Service.TaskSevice
{
    public class TasksService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<TaskResponseDTO>> AddNewTask(TaskDTO dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<TaskResponseDTO>.Fail("Unauthorized");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
                return ServiceResponse<TaskResponseDTO>.Fail("Project not found");

            if (dto.Deadline < DateOnly.FromDateTime(DateTime.UtcNow))
                return ServiceResponse<TaskResponseDTO>.Fail("Deadline must be in the future");

            var task = new WorkTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                CreatedBy = userId,
                Status = TasksStatus.ToDo,
                CreatedOn = DateTime.UtcNow,
                ProjectId = dto.ProjectId,
                priority = dto.priority,
                AssignedUserId = dto.AssignedUserId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return ServiceResponse<TaskResponseDTO>.SuccessResponse(
                new TaskResponseDTO
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    Priority = task.priority,
                    Deadline = task.Deadline,
                    ProjectId = task.ProjectId,
                    AssignedUserName = null
                },
                "Task created successfully"
            );
        }
        public async Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetAllTasks(int projectId, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<IEnumerable<TaskResponseDTO>>.Fail("Unauthorized");

            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            var result = tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.priority,
                Deadline = t.Deadline,
                ProjectId = t.ProjectId,
                AssignedUserName = t.AssignedUser == null
                    ? null
                    : $"{t.AssignedUser.FName} {t.AssignedUser.LName}"
            });

            return ServiceResponse<IEnumerable<TaskResponseDTO>>
                .SuccessResponse(result);
        }
        public async Task<ServiceResponse<TaskResponseDTO>> GetTaskById(int projectId, int taskId, ClaimsPrincipal user)
        {
            var task = await _context.Tasks.Include(t => t.AssignedUser)
                      .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);

            if (task == null)
                return ServiceResponse<TaskResponseDTO>.Fail("Task not found");

            var result = new TaskResponseDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.priority,
                Deadline = task.Deadline,
                ProjectId = task.ProjectId,
                AssignedUserName = task.AssignedUser == null
                    ? null
                    : $"{task.AssignedUser.FName} {task.AssignedUser.LName}"
            };

            return ServiceResponse<TaskResponseDTO>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<string>> EditTaskStatus(int projectId, int taskId, ClaimsPrincipal user)
        {

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<string>.Fail("Unauthorized");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (task == null)
                return ServiceResponse<string>.Fail("Task not found");

            if (!user.IsInRole("Admin") && task.AssignedUserId != userId)
                return ServiceResponse<string>.Fail("Forbidden");


            task.Status = task.Status switch
            {
                TasksStatus.ToDo => TasksStatus.InProgress,
                TasksStatus.InProgress => TasksStatus.Done,
                _ => task.Status
            };

            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Updated", "Status updated");
        }

        public async Task<ServiceResponse<string>> DeleteTask(int taskId, ClaimsPrincipal user)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return ServiceResponse<string>.Fail("Task not found");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Deleted", "Deleted successfully");
        }

        public async Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetUsersTasks(string userId, ClaimsPrincipal user)
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return ServiceResponse<IEnumerable<TaskResponseDTO>>.Fail("Unauthorized");

            if (!user.IsInRole("Admin") && currentUserId != userId)
                return ServiceResponse<IEnumerable<TaskResponseDTO>>.Fail("Forbidden");
            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .ToListAsync();

            var result = tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.priority,
                Deadline = t.Deadline,
                ProjectId = t.ProjectId,
                AssignedUserName = t.AssignedUser == null
                    ? null
                    : $"{t.AssignedUser.FName} {t.AssignedUser.LName}"
            });

            return ServiceResponse<IEnumerable<TaskResponseDTO>>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetTaskCreator( ClaimsPrincipal user)
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return ServiceResponse<IEnumerable<TaskResponseDTO>>.Fail("Unauthorized");

       
            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.CreatedBy == currentUserId)
                .ToListAsync();

            var result = tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.priority,
                Deadline = t.Deadline,
                ProjectId = t.ProjectId,
                AssignedUserName = t.AssignedUser == null
                    ? null
                    : $"{t.AssignedUser.FName} {t.AssignedUser.LName}"
            });

            return ServiceResponse<IEnumerable<TaskResponseDTO>>
                .SuccessResponse(result, "User tasks retrieved successfully");
        }

        public async Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetTasks(int? projectId, TasksStatus? status, Priority? priority, ClaimsPrincipal user)
        {
            var query = _context.Tasks.Include(t => t.AssignedUser).AsQueryable();

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status);

            if (priority.HasValue)
                query = query.Where(t => t.priority == priority);

            var tasks = await query.ToListAsync();

            var result = tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.priority,
                Deadline = t.Deadline,
                ProjectId = t.ProjectId,
                AssignedUserName = t.AssignedUser == null
                    ? null
                    : $"{t.AssignedUser.FName} {t.AssignedUser.LName}"
            });

            return ServiceResponse<IEnumerable<TaskResponseDTO>>.SuccessResponse(result);
        }
    }
}
