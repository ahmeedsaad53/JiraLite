using Humanizer;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JiraLiteAPI.Data.Models;


namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public DashboardController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var totalProjects = await _Context.Projects.CountAsync();
            var totalTasks = await _Context.Tasks.CountAsync();
            var totalUsers = await _Context.Users.CountAsync();
            var totalComments = await _Context.Comments.CountAsync();
            var totalAttachment = await _Context.Attachments.CountAsync();


            var pendingRequests = await _Context.TaskRequests
                .CountAsync(r => r.Status == RequestStatus.pending);

            var toDo = await _Context.Tasks
                .CountAsync(t => t.Status == TasksStatus.ToDo);

            var inProgress = await _Context.Tasks
                .CountAsync(t => t.Status == TasksStatus.InProgress);

            var done = await _Context.Tasks
                .CountAsync(t => t.Status == TasksStatus.Done);

            var recentActivity = await _Context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new
                {
                    a.Description,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalProjects,
                totalTasks,
                totalUsers,
                pendingRequests,

                tasksStatus = new
                {
                    toDo,
                    inProgress,
                    done
                },

                recentActivity
            });
        }


        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var myTasks = await _Context.Tasks
                .CountAsync(t => t.AssignedUserId == userId);

            var myPendingRequests = await _Context.TaskRequests
                .CountAsync(r => r.UserId == userId && r.Status == RequestStatus.pending);
            var myComments = await _Context.Comments
                .CountAsync(c => c.UserId == userId);

            var myToDo = await _Context.Tasks
                .CountAsync(t => t.AssignedUserId == userId && t.Status == TasksStatus.ToDo);

            var myInProgress = await _Context.Tasks
                .CountAsync(t => t.AssignedUserId == userId && t.Status == TasksStatus.InProgress);

            var myDone = await _Context.Tasks
                .CountAsync(t => t.AssignedUserId == userId && t.Status == TasksStatus.Done);
            var recentActivity = await _Context.ActivityLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new
                {
                    a.Description,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                myTasks,
                myPendingRequests,
                myComments,

                myTasksStatus = new
                {
                    toDo = myToDo,
                    inProgress = myInProgress,
                    done = myDone
                },

                recentActivity
            });
        }












    }
}
