using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Service.ActivityLogService
{
    public class ActivityLogServices:IActivityLogService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        public ActivityLogServices(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }




        public async Task<ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>> GetAllLogs( int? taskId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _context.ActivityLogs.Include(a => a.User).AsQueryable();

            if (taskId.HasValue)
                query = query.Where(a => a.TaskId == taskId);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActivityLogResponseDTO
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    Action = a.Action.ToString(),
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    UserFullName = a.User == null
                        ? ""
                        : $"{a.User.FName} {a.User.LName}"
                })
                .ToListAsync();

            var result = new PaginatedResponseDTO<ActivityLogResponseDTO>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Data = logs
            };

            return ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>
                .SuccessResponse(result);
        }








        public async Task<ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>> GetMyLogs( ClaimsPrincipal user, int? taskId, int page, int pageSize)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>.Fail("Unauthorized");

            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _context.ActivityLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId);

            if (taskId.HasValue)
                query = query.Where(a => a.TaskId == taskId);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActivityLogResponseDTO
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    Action = a.Action.ToString(),
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    UserFullName = a.User == null
                        ? ""
                        : $"{a.User.FName} {a.User.LName}"
                })
                .ToListAsync();

            var result = new PaginatedResponseDTO<ActivityLogResponseDTO>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Data = logs
            };

            return ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>
                .SuccessResponse(result);
        }




























    }
}
