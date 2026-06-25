using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Service.TaskRequestService
{
    public class TaskRequestService : ITaskRequestService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskRequestService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<CreateTaskRequestResponseDTO>> CreateTaskRequest(TaskRequestDTO dto, ClaimsPrincipal user)
        {
            if (dto == null)
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Invalid request");

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);  
            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Unauthorized");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == dto.TaskId);
            if (task == null)
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Task not found");

            var isMember = await _context.ProjectUsers
                .AnyAsync(p => p.UserId == userId && p.ProjectId == task.ProjectId);

            if (!isMember)
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Not part of project");

            if (task.AssignedUserId != null)
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Task already assigned");

            if (task.Status != TasksStatus.ToDo)
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Task not available");

            var oldRequest = await _context.TaskRequests
                .FirstOrDefaultAsync(r => r.TaskId == dto.TaskId && r.UserId == userId);

            if (oldRequest != null)
                return ServiceResponse<CreateTaskRequestResponseDTO>.Fail("Already requested");

            var request = new TaskRequest
            {
                TaskId = dto.TaskId,
                UserId = userId,
                Status = RequestStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.TaskRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            return ServiceResponse<CreateTaskRequestResponseDTO>.SuccessResponse(
                new CreateTaskRequestResponseDTO { RequestId = request.Id },
                "Request created"
            );
        }

        public async Task<ServiceResponse<PaginatedResponse<TaskRequestItemDTO>>> GetRequests(
            RequestStatus? status, int? taskId, int page, int pageSize)
        {
            var query = _context.TaskRequests.AsQueryable();

            if (!status.HasValue)
                query = query.Where(r => r.Status == RequestStatus.pending);
            else
                query = query.Where(r => r.Status == status);

            if (taskId.HasValue)
                query = query.Where(r => r.TaskId == taskId);

            var total = await query.CountAsync();

            var data = await query
                .Include(r => r.WorkTask)
                .Include(r => r.User)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PaginatedResponse<TaskRequestItemDTO>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Data = data.Select(r => new TaskRequestItemDTO
                {
                    Id = r.Id,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    Task = r.WorkTask == null ? null : new TaskMiniDTO
                    {
                        Id = r.WorkTask.Id,
                        Title = r.WorkTask.Title
                    },
                    User = r.User == null ? null : new UserMiniDTO
                    {
                        Id = r.User.Id,
                        FullName = (r.User.FName ?? "") + " " + (r.User.LName ?? "")
                    }
                }).ToList()
            };

            return ServiceResponse<PaginatedResponse<TaskRequestItemDTO>>
                .SuccessResponse(result);
        }

        public async Task<ServiceResponse<HandleRequestResponseDTO>> HandleRequest(
            HandleRequestDTO dto, int requestId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<HandleRequestResponseDTO>.Fail("Unauthorized");

            var request = await _context.TaskRequests
                .Include(r => r.WorkTask)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return ServiceResponse<HandleRequestResponseDTO>.Fail("Request not found");

            if (request.Status != RequestStatus.pending)
                return ServiceResponse<HandleRequestResponseDTO>.Fail("Already handled");

            if (dto.Status == ApproveRequests.Accepted)
            {
                if (request.WorkTask.AssignedUserId != null)
                    return ServiceResponse<HandleRequestResponseDTO>.Fail("Already assigned");

                request.WorkTask.AssignedUserId = request.UserId;
                request.WorkTask.Status = TasksStatus.InProgress;
                request.Status = RequestStatus.accepted;
            }
            else if (dto.Status == ApproveRequests.Rejected)
            {
                request.Status = RequestStatus.rejected;
            }

            await _context.SaveChangesAsync();

            return ServiceResponse<HandleRequestResponseDTO>.SuccessResponse(
                new HandleRequestResponseDTO
                {
                    RequestId = request.Id,
                    Status = request.Status
                });
        }

        public async Task<ServiceResponse<List<MyRequestDTO>>> GetMyRequests(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<List<MyRequestDTO>>.Fail("Unauthorized");

            var data = await _context.TaskRequests
                .Where(r => r.UserId == userId)
                .Select(r => new MyRequestDTO
                {
                    Id = r.Id,
                    Status = r.Status,
                    Task = r.WorkTask == null ? null : new TaskInfoDTO
                    {
                        Id = r.WorkTask.Id,
                        Title = r.WorkTask.Title,
                        Description = r.WorkTask.Description,
                        Status = r.WorkTask.Status,
                        Deadline = r.WorkTask.Deadline
                    }
                }).ToListAsync();

            return ServiceResponse<List<MyRequestDTO>>
                .SuccessResponse(data);
        }

        public async Task<ServiceResponse<string>> DeleteRequest(int requestId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<string>.Fail("Unauthorized");

            var request = await _context.TaskRequests.FindAsync(requestId);
            if (request == null)
                return ServiceResponse<string>.Fail("Request not found");

            if (!user.IsInRole("Admin") && request.UserId != userId)
                return ServiceResponse<string>.Fail("Access denied");

            _context.TaskRequests.Remove(request);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Deleted successfully");
        }
    }
}
