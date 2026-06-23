using Humanizer;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Service.ProjectUsersService;
using JiraLiteAPI.Service.TaskSevice;
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

        private readonly IProjectUserService _ProjectUserService;

        public ProjectUserController(IProjectUserService ProjectUserService)
        {
            _ProjectUserService = ProjectUserService;
        }

        [HttpPost("AddUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser(string userId, [FromBody] AddUserToProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
           var result= await _ProjectUserService.AddUser(userId, dto);
            return Ok(result);

        }
        [HttpDelete("remove-user/{projectId:int}/{userId}")]
        [Authorize (Roles =("Admin"))]
        public async Task<IActionResult> DeleteUserFromProject( int projectId,string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _ProjectUserService.DeleteUserFromProject(projectId, userId);
            return Ok(result);

        }


        [HttpGet("{projectId:int}/users")]
        [Authorize]
        public async Task<IActionResult> GetAllUser(int projectId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _ProjectUserService.GetAllUser(projectId,User);
            return Ok(result);

        }





    }
}
