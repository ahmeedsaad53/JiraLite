using Humanizer;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Security.Claims;


namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashBoardService _dashboardService;
        public DashboardController(IDashBoardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _dashboardService.GetAdminDashboard();
            return Ok(result);
        }


        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserDashboard()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _dashboardService.GetUserDashboard(User);
            return Ok(result);
        }












    }
}
