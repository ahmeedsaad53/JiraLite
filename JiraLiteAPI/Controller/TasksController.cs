using Humanizer;
using JiraLiteAPI.Data;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using JiraLiteAPI.Service.PService;
using JiraLiteAPI.Service.TaskSevice;
using JiraLiteAPI.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {

        private readonly ITaskService _taskService ;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }


        [HttpPost] // add new task
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddNewTask(TaskDTO taskDTO)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.AddNewTask(taskDTO, User);

            return Ok(result);
        }


        //get the project task

        [HttpGet("{projectId:int}/Tasks")]
        [Authorize]
        public async Task<IActionResult> GetAllTasks(int projectId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.GetAllTasks(projectId, User);

            return Ok(result);
        }
         
        //get task by id

        [HttpGet("{projectId:int}/tasks/{taskId:int}")]
        [Authorize]
        public async Task<IActionResult> GetTasksById(int projectId, int taskId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.GetTasksById(projectId, taskId, User);

            return Ok(result);

        }


        //change the task stauts

        [HttpPatch("{projectId:int}/tasks/{taskId:int}")]
        [Authorize]
        public async Task<IActionResult>EditTaskStauts(int projectId, int taskId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.EditTaskStauts(projectId, taskId, User);

            return Ok(result);
        }


        //delete task

        [HttpDelete("{taskId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.DeleteTask( taskId, User);

            return Ok(result);
        }





        //get the user tasks
        [HttpGet("user/{userId}/tasks")]
        [Authorize]
        public async Task<IActionResult>GetUsersTasks(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.GetUsersTasks(userId, User);

            return Ok(result);

        }




        //get the all task for Creator

        [HttpGet("TaskCreator/{createdBy}")]
        [Authorize]
        public async Task<IActionResult> GetTaskCreator(string createdBy)
        {
           if(!ModelState.IsValid) 
                return BadRequest(ModelState);
        
            var result = await _taskService.GetTaskCreator(createdBy, User);
             return Ok(result);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTasks(int? projectId,TasksStatus? status, Priority? priority)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.GetTasks(projectId, status, priority, User);
            return Ok(result);

        }




















    }
}
