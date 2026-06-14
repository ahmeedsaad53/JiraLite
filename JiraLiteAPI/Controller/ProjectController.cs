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
    public class ProjectController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public ProjectController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddNewProject(ProjectDTO projectDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;//get user id from token 
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if(projectDTO.DeadLine < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest("Deadline must be in the future");
            var NewProject = new Project
            {
                Name = projectDTO.Name,
                Description = projectDTO.Description,
                CreatedBy = userId,
                DeadLine = projectDTO.DeadLine,
                Status = projectDTO.Status,
                CreatedOn = DateTime.Now
            };
            _Context.Projects.Add(NewProject);
            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "Project created successfully",
                projectId = NewProject.Id
            });
        }
        [HttpGet]
        [Authorize]
       public async Task<IActionResult> GetAllProjects()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            if (User.IsInRole("Admin"))
            {
                var projects = await _Context.Projects
                    .OrderByDescending(p => p.CreatedOn)
                    .Join(_Context.Users, p => p.CreatedBy, u => u.Id, (p, u) => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        CreatedByName = (u.FName ?? "") + " " + (u.LName ?? ""),
                        p.CreatedOn,
                        p.DeadLine,
                        p.Status
                    }).ToListAsync();
                return Ok(projects);
            }
            var UserProject=_Context.ProjectUsers.Where(pu=>pu.UserId == userId).Select(pu => new
            {

                pu.Project.Id,
                pu.Project.Name,
                pu.Project.Description,
                pu.Project.CreatedOn,
                pu.Project.DeadLine,
                pu.Project.Status
            }).ToListAsync();
            return Ok(UserProject);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetProjectById (int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId);

                if (!isMember)
                    return Forbid();
            }
            var project = await _Context.Projects.Join(_Context.Users,p=>p.CreatedBy,u=>u.Id , (p,u) => new
            {
                p.Id,
                p.Name,
                p.CreatedOn,
                p.DeadLine,
                p.Description,
                CreatedByName = (u.FName ?? "") + " " + (u.LName ?? ""),
                p.Status
            }).FirstOrDefaultAsync(p=>p.Id== id);
            if (project == null) return NotFound("Project not found");
            return Ok(project);
        }
        [HttpPatch("{id:int}")]
        [Authorize]

        public async Task<IActionResult> EditProjectStatus(int id,UpdateProjectProgressDTO updateProjectProgressDTO) {

            if(!ModelState.IsValid)
        return BadRequest(ModelState);


            var Project =await _Context.Projects.FirstOrDefaultAsync(p=>p.Id== id);
            if (Project == null) return NotFound();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == id && pu.UserId == userId);

                if (!isMember)
                    return Forbid();
            }
            Project.Status = updateProjectProgressDTO.ProjectStatus;
            Project.Description = updateProjectProgressDTO.Description;
             await _Context.SaveChangesAsync();
            return Ok("Project status updated successfully");
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditProject(int id, EditProjectDTO editProjectDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var Project = await _Context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (Project == null) return NotFound();
            
            if (editProjectDTO.DeadLine < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest("Deadline must be in the future");
            Project.Name = editProjectDTO.Name;
            Project.Status = editProjectDTO.Status;
            Project.DeadLine = editProjectDTO.DeadLine;
            Project.Description = editProjectDTO.Description;
            await _Context.SaveChangesAsync();
            return Ok("Project Updated Successfully");
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult>DeleteProject(int id)
        {
            var Project = await _Context.Projects.FirstOrDefaultAsync(p=>p.Id == id);
            if(Project== null) return NotFound();
            _Context.Projects.Remove(Project);
            await _Context.SaveChangesAsync();
            return Ok(new
            {
                message = "Project deleted successfully",
                projectId = id
            });  
        }














    }
}

