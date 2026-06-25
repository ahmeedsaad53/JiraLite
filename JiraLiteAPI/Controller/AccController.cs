using JiraLiteAPI.Controller;
using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AccController : BaseController
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
        return HandleResponse(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var result = await _accountService.Login(dto);
        return HandleResponse(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _accountService.DeleteUser(id);
        return HandleResponse(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _accountService.GetAllUser();
        return HandleResponse(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _accountService.GetById(id);
        return HandleResponse(result);
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(AssignRoleDTO dto)
    {
        var result = await _accountService.AssignRole(dto);
        return HandleResponse(result);
    }
}