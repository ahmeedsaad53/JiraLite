using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Service.ProjectUsersService
{
    public class ProjectUserService: IProjectUserService
    {
        private readonly AppDbContext _Context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectUserService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _Context = context;
            _userManager = userManager;

        }
        // add user to project
        public async Task<object> AddUser(string userId,  AddUserToProjectDTO dto)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var project = await _Context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
                throw new Exception("Project not found");

            var exists = await _Context.ProjectUsers
                .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == dto.ProjectId);

            if (exists)
                throw new Exception ("User already in project");

            var userProject = new ProjectUser
            {
                UserId = userId,
                ProjectId = dto.ProjectId
            };

            _Context.ProjectUsers.Add(userProject);
            await _Context.SaveChangesAsync();

            return new
            {
                message = "User added to project successfully",
                userId = userId,
                projectId = dto.ProjectId
            };
        }



        // delete user from project 
        public async Task<object> DeleteUserFromProject(int projectId, string userId)
        {
            var user = await _Context.ProjectUsers.FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == projectId);
            if (user == null)throw new Exception ("User Not Found");
            _Context.ProjectUsers.Remove(user);
            await _Context.SaveChangesAsync();
            return new
            {
                message = "User deleted successfully",
                UserId = userId,
                projectId
            };
        }


        //get all user for project 
        public async Task<object> GetAllUser(int projectId, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                throw new UnauthorizedAccessException();

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (!isMember)
                    throw new UnauthorizedAccessException();
            }

            var ProjectUsers = await _Context.ProjectUsers.Where(p => p.ProjectId == projectId).Join(_Context.Users,
                pu => pu.UserId,
                u => u.Id,
                (pu, u) => new
                {
                    u.Id,
                    FullName = (u.FName ?? "") + " " + (u.LName ?? "")
                }).ToListAsync();
            return ProjectUsers;
        }
































    }
}
