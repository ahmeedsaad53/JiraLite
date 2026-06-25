using JiraLiteAPI.DTO;
using JiraLiteAPI.Service.PService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProject(ProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _projectService.CreateProject(dto, User);

            return HandleResponse(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProjects()
        {
            var result = await _projectService.GetAllProjects(User);

            return HandleResponse(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var result = await _projectService.GetProjectById(id, User);

            return HandleResponse(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject(int id, EditProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _projectService.UpdateProject(id, dto);

            return HandleResponse(result);
        }

        [HttpPatch("{id:int}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateProjectStatus(int id, UpdateProjectProgressDTO dto)
        {
            var result = await _projectService.UpdateProjectStatus(id, dto, User);

            return HandleResponse(result);
        }
     
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var result = await _projectService.DeleteProject(id);

            return HandleResponse(result);
        }







    }
}


