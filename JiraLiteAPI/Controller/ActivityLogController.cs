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
    public class ActivityLogController : BaseController
    {
        private readonly IActivityLogService _service;

        public ActivityLogController(IActivityLogService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLogs(int? taskId, int page = 1, int pageSize = 10)
        {
            var result = await _service.GetAllLogs(taskId, page, pageSize);
            return HandleResponse(result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyLogs(int? taskId, int page = 1, int pageSize = 10)
        {
            var result = await _service.GetMyLogs(User, taskId, page, pageSize);
            return HandleResponse(result);
        }
    }


}

