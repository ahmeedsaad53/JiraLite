using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Service.ProjectUsersService
{
    public class ProjectUserService : IProjectUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectUserService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<string>> AddUser(string userId, AddUserToProjectDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResponse<string>.Fail("User not found");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
                return ServiceResponse<string>.Fail("Project not found");

            var exists = await _context.ProjectUsers
                .AnyAsync(p => p.UserId == userId && p.ProjectId == dto.ProjectId);

            if (exists)
                return ServiceResponse<string>.Fail("User already in project");

            var userProject = new ProjectUser
            {
                UserId = userId,
                ProjectId = dto.ProjectId
            };

            _context.ProjectUsers.Add(userProject);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse(
                "Added",
                "User added to project successfully"
            );
        }

        public async Task<ServiceResponse<string>> DeleteUserFromProject(int projectId, string userId)
        {
            var user = await _context.ProjectUsers
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == projectId);

            if (user == null)
                return ServiceResponse<string>.Fail("User not found in project");

            _context.ProjectUsers.Remove(user);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse(
                "Deleted",
                "User removed from project"
            );
        }

        public async Task<ServiceResponse<IEnumerable<ProjectUserResponseDTO>>> GetAllUser(int projectId, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<IEnumerable<ProjectUserResponseDTO>>.Fail("Unauthorized");

            if (!user.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == projectId && p.UserId == userId);

                if (!isMember)
                    return ServiceResponse<IEnumerable<ProjectUserResponseDTO>>.Fail("Forbidden");
            }

            var users = await _context.ProjectUsers
                .Where(p => p.ProjectId == projectId)
                .Join(_context.Users,
                    pu => pu.UserId,
                    u => u.Id,
                    (pu, u) => new ProjectUserResponseDTO
                    {
                        UserId = u.Id,
                        FullName = (u.FName ?? "") + " " + (u.LName ?? "")
                    })
                .ToListAsync();

            return ServiceResponse<IEnumerable<ProjectUserResponseDTO>>
                .SuccessResponse(users, "Users retrieved");
        }









    }

    }


