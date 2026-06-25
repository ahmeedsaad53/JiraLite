using JiraLiteAPI.Controller;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class TasksRequestController : BaseController
{
    private readonly ITaskRequestService _taskrequestService;

    public TasksRequestController(ITaskRequestService taskrequestService)
    {
        _taskrequestService = taskrequestService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTaskRequest(TaskRequestDTO dto)
    {
        var result = await _taskrequestService.CreateTaskRequest(dto, User);
        return HandleResponse(result);
    }



    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRequests(RequestStatus? status, int? taskId, int page = 1, int pageSize = 10)
    {
        var result = await _taskrequestService.GetRequests(status, taskId, page, pageSize);
        return HandleResponse(result);
    }



    [HttpPatch("{requestId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> HandleRequest(int requestId, HandleRequestDTO dto)
    {
        var result = await _taskrequestService.HandleRequest(dto, requestId, User);
        return HandleResponse(result);
    }






    [HttpGet("my-requests")]
    [Authorize]
    public async Task<IActionResult> GetMyRequests()
    {
        var result = await _taskrequestService.GetMyRequests(User);
        return HandleResponse(result);
    }





       
    [HttpDelete("{requestId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        var result = await _taskrequestService.DeleteRequest(requestId, User);
        return HandleResponse(result);
    }
}