using Azure.Core;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using JiraLiteAPI.Service.TaskRequestService;
using JiraLiteAPI.Service.TaskSevice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksRequestController : ControllerBase
    {
        private readonly ITaskRequestService _taskrequestService;

        public TasksRequestController(ITaskRequestService taskrequestService)
        {
            _taskrequestService = taskrequestService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTaskRequest(TaskRequestDTO taskRequestDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskrequestService.CreateTaskRequest(taskRequestDTO, User);

            return Ok(result);
        }



        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRequests(RequestStatus? status,int? taskId,int page = 1,int pageSize = 10)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskrequestService.GetRequests(status, taskId, page, pageSize);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}")]
        [Authorize (Roles =("Admin"))]
        public async Task<IActionResult> HandleRequest(HandleRequestDTO dto,int requestId )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskrequestService.HandleRequest(dto,requestId, User);

            return Ok(result);
        }


        [HttpGet("myTasks")]
        [Authorize]
        public async Task<IActionResult> GetMyRequests()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskrequestService.GetMyRequests( User);

            return Ok(result);
        }

        [HttpDelete("{RequestId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteRequest(int RequestId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskrequestService.DeleteRequest(RequestId, User);

            return Ok(result);

        }
















    }
}
