using JiraLiteAPI.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.Service.ActivityLogService;


namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;
        public ActivityLogController(IActivityLogService activityLogService)
        {

            _activityLogService = activityLogService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLogs(int? taskId, int page = 1, int pageSize = 10)
        {
            if(!ModelState.IsValid) 
                return BadRequest(ModelState);
            var result= await _activityLogService.GetAllLogs(taskId, page, pageSize);
            return Ok(result);

        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyLogs(int? taskId, int page = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _activityLogService.GetMyLogs(User, taskId, page, pageSize);
            return Ok(result);

        }


    }
}
