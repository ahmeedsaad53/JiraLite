using JiraLiteAPI.DTO;
using JiraLiteAPI.Service.AccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var result = await _accountService.Register(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var result = await _accountService.Login(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _accountService.DeleteUser(id);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _accountService.GetAllUser();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _accountService.GetById(id);
            return Ok(result);
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(AssignRoleDTO dto)
        {
            var result = await _accountService.AssignRole(dto);
            return Ok(result);
        }
    }
}