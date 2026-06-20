using JiraLiteAPI.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JiraLiteAPI.Data.Models;


namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public ActivityLogController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLogs(int? taskId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _Context.ActivityLogs.AsQueryable();

            if (taskId.HasValue)
                query = query.Where(a => a.TaskId == taskId);

            var totalCount = await query.CountAsync();
            var activityLogs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.TaskId,
                    a.Action,
                    a.Description,
                    a.CreatedAt,
                    User = a.User == null ? null : new
                    {
                        a.User.Id,
                        FullName = (a.User.FName ?? "") + " " + (a.User.LName ?? "")
                    }
                }).ToListAsync();

            return Ok(new
            {
                page, pageSize,totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = activityLogs
            });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyLogs(int? taskId, int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _Context.ActivityLogs
                .Where(a => a.UserId == userId);

            if (taskId.HasValue)
                query = query.Where(a => a.TaskId == taskId);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.TaskId,
                    a.Action,
                    a.Description,
                    a.CreatedAt,

                    User = a.User == null ? null : new
                    {
                        a.User.Id,
                        FullName = (a.User.FName ?? "") + " " + (a.User.LName ?? "")
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = logs
            });
        }


    }
}
