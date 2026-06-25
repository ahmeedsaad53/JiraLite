using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class DashboradService : IDashBoardService
{
    private readonly AppDbContext _context;

    public DashboradService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<AdminDashboardDTO>> GetAdminDashboard()
    {
        var totalProjects = await _context.Projects.CountAsync();
        var totalTasks = await _context.Tasks.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var pendingRequests = await _context.TaskRequests
            .CountAsync(r => r.Status == RequestStatus.pending);

        var toDo = await _context.Tasks.CountAsync(t => t.Status == TasksStatus.ToDo);
        var inProgress = await _context.Tasks.CountAsync(t => t.Status == TasksStatus.InProgress);
        var done = await _context.Tasks.CountAsync(t => t.Status == TasksStatus.Done);

        var recentActivity = await _context.ActivityLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new ActivityDTO
            {
                Description = a.Description,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var result = new AdminDashboardDTO
        {
            TotalProjects = totalProjects,
            TotalTasks = totalTasks,
            TotalUsers = totalUsers,
            PendingRequests = pendingRequests,

            TasksStatus = new TasksStatusSummary
            {
                ToDo = toDo,
                InProgress = inProgress,
                Done = done
            },

            RecentActivity = recentActivity
        };

        return ServiceResponse<AdminDashboardDTO>
            .SuccessResponse(result, "Admin dashboard loaded");
    }

    public async Task<ServiceResponse<UserDashboardDTO>> GetUserDashboard(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return ServiceResponse<UserDashboardDTO>.Fail("Unauthorized");

        var myTasks = await _context.Tasks.CountAsync(t => t.AssignedUserId == userId);
        var myPendingRequests = await _context.TaskRequests
            .CountAsync(r => r.UserId == userId && r.Status == RequestStatus.pending);

        var myComments = await _context.Comments.CountAsync(c => c.UserId == userId);

        var myToDo = await _context.Tasks
            .CountAsync(t => t.AssignedUserId == userId && t.Status == TasksStatus.ToDo);

        var myInProgress = await _context.Tasks
            .CountAsync(t => t.AssignedUserId == userId && t.Status == TasksStatus.InProgress);

        var myDone = await _context.Tasks
            .CountAsync(t => t.AssignedUserId == userId && t.Status == TasksStatus.Done);

        var recentActivity = await _context.ActivityLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new ActivityDTO
            {
                Description = a.Description,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var result = new UserDashboardDTO
        {
            MyTasks = myTasks,
            MyPendingRequests = myPendingRequests,
            MyComments = myComments,

            MyTasksStatus = new TasksStatusSummary
            {
                ToDo = myToDo,
                InProgress = myInProgress,
                Done = myDone
            },

            RecentActivity = recentActivity
        };

        return ServiceResponse<UserDashboardDTO>
            .SuccessResponse(result, "User dashboard loaded");
    }
}