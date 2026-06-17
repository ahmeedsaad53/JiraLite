using JiraLiteAPI.Data;
using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectUserController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public ProjectUserController (UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }
        [HttpPost("AddUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser(string userId, [FromBody] AddUserToProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var project = await _Context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
                return NotFound("Project not found");

            var exists = await _Context.ProjectUsers
                .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == dto.ProjectId);

            if (exists)
                return BadRequest("User already in project");

            var userProject = new ProjectUser
            {
                UserId = userId, 
                ProjectId = dto.ProjectId
            };

            _Context.ProjectUsers.Add(userProject);
            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "User added to project successfully",
                userId = userId,
                projectId = dto.ProjectId
            });
        }
        [HttpDelete("remove-user/{projectId:int}/{userId}")]
        [Authorize (Roles =("Admin"))]
        public async Task<IActionResult> DeleteUserFromProject( int projectId,string userId)
        {
            var user= await _Context.ProjectUsers.FirstOrDefaultAsync(p=>p.UserId== userId && p.ProjectId== projectId);
            if (user == null) return NotFound();
            _Context.ProjectUsers.Remove(user);
            await _Context. SaveChangesAsync();
            return Ok(new
            {
                message = "User deleted successfully",
                UserId = userId  ,projectId
            });
        }


        [HttpGet("{projectId:int}/users")]
        [Authorize]
        public async Task<IActionResult> GetAllUser(int projectId)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (!isMember)
                    return Forbid();
            }

            var ProjectUsers = await _Context.ProjectUsers.Where(p => p.ProjectId == projectId).Join(_Context.Users,
                pu => pu.UserId,
                u => u.Id,
                (pu, u) => new
                {
                    u.Id,
                    FullName = (u.FName ?? "") + " " + (u.LName ?? "")
                }).ToListAsync();
            return Ok( ProjectUsers);
        }





    }
}
