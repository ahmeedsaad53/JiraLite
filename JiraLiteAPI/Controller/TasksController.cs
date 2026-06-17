using JiraLiteAPI.Data;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
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
    public class TasksController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public TasksController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }


        [HttpPost] // add new task
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddNewTask(TaskDTO taskDTO)
        {
            //cheak the user id form the token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            //cheak if Project is found
            var project = await _Context.Projects
                .FirstOrDefaultAsync(p => p.Id == taskDTO.ProjectId);

            if (project == null)
                return NotFound("Project not found");
            //cheak if project are completed or cancelled
            if (project.Status == ProjectStatus.Completed ||
                project.Status == ProjectStatus.Cancelled)
                return BadRequest("Project already finished");
            //cheak if the deadline not  in furure
            if (taskDTO.Deadline < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest("Deadline must be in the future");

            string? assignedUserId = null;
            //if the task is hard will make all this cheak to give it to the best user to do it is easy will let it the other users to take it 
            if (taskDTO.IsHard)
            {
                if (string.IsNullOrEmpty(taskDTO.AssignedUserId))//cheak if used id in dto is null
                    return BadRequest("AssignedUserId is required");

                var user = await _userManager.FindByIdAsync(taskDTO.AssignedUserId);//cheak if user id already in the db

                if (user == null)
                    return BadRequest("User not found");

                var isMember = await _Context.ProjectUsers // cheak if the user id already in the project 
                    .AnyAsync(pu => pu.ProjectId == taskDTO.ProjectId &&
                                    pu.UserId == taskDTO.AssignedUserId);

                if (!isMember)
                    return BadRequest("User not in project");

                assignedUserId = taskDTO.AssignedUserId;//give task for the user
            }

            var newTask = new WorkTask
            {
                Title = taskDTO.Title,
                Description = taskDTO.Description,
                Deadline = taskDTO.Deadline,
                CreatedBy = userId,
                Status = TasksStatus.ToDo,
                CreatedOn = DateTime.Now,
                ProjectId = taskDTO.ProjectId,
                priority = taskDTO.priority,
                AssignedUserId = assignedUserId
            };
            _Context.Tasks.Add(newTask);
            await _Context.SaveChangesAsync();

            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = newTask.Id,
                UserId = userId,
                Action = ActivityType.CreatedTask,
                Description = $"User created task '{newTask.Title}' in project {taskDTO.ProjectId}",
                CreatedAt = DateTime.UtcNow
            });
            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "Task created successfully",
                taskId = newTask.Id
            });
        }


        //get the project task

        [HttpGet("{projectId:int}/Tasks")]
        [Authorize]
        public async Task<IActionResult> GetAllTasks(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();
            var Project =await _Context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (Project == null) return NotFound("Project Not Found");


            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (!isMember)
                    return Forbid();

            }
            var projectTasks = await _Context.Tasks.Where(t => t.ProjectId == projectId)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.priority,
                t.Deadline,
                t.ProjectId,  
                AssignedUser = t.AssignedUser == null ? null : new
                {
                    t.AssignedUser.Id,
                    FullName = (t.AssignedUser.FName ?? "") + " " + (t.AssignedUser.LName ?? "")
                } })
            .ToListAsync();

            return Ok(projectTasks);
        }
         
        //get task by id

        [HttpGet("{projectId:int}/tasks/{taskId:int}")]
        [Authorize]
        public async Task<IActionResult> GetTasksById(int projectId, int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();
            var project = await _Context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound("Project Not Found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (!isMember)
                    return Forbid();

            }
            var task = await _Context.Tasks.Where(t => t.ProjectId == projectId && t.Id == taskId)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.priority,
                t.Deadline,
                t.ProjectId,
               
                AssignedUser = t.AssignedUser == null ? null : new
                {
                    t.AssignedUser.Id,
                    FullName = (t.AssignedUser.FName ?? "") + " " + (t.AssignedUser.LName ?? "")
                }
                 
            }).FirstOrDefaultAsync();

            if (task == null)
                return NotFound("Task not found");

            return Ok(task);
        }


        //change the task stauts

        [HttpPatch("{projectId:int}/tasks/{taskId:int}")]
        [Authorize]
        public async Task<IActionResult>EditTaskStauts(int projectId, int taskId)
        {
            var userId=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(userId == null) return Unauthorized();

            var project = await _Context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound("Project Not Found");

            var task = await _Context.Tasks.Where(pu => pu.ProjectId == projectId && pu.Id == taskId).FirstOrDefaultAsync();

            if (task == null) return NotFound();

            if (!User.IsInRole("Admin") && task.AssignedUserId != userId) return Forbid(); // Only assigned user or admin

            if (task.Status == TasksStatus.Done)
                return BadRequest("Task already completed");

            if (task.Status == TasksStatus.ToDo)
            {
                task.Status = TasksStatus.InProgress;
                await _Context.SaveChangesAsync();
                _Context.ActivityLogs.Add(new ActivityLog
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Action = ActivityType.UpdatedStatus,
                    Description = $"User Edited the Status To Task{task.Title} From ToDo To InProgress ",
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (task.Status == TasksStatus.InProgress)
            {
                task.Status = TasksStatus.Done;
                await _Context.SaveChangesAsync();
                _Context.ActivityLogs.Add(new ActivityLog
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Action = ActivityType.UpdatedStatus,
                    Description = $"User Edited the Status To Task{task.Title} From InProgress To Done ",
                    CreatedAt = DateTime.UtcNow
                });
                await _Context.SaveChangesAsync();

            }


            return Ok(new
            {
                message = "Task status updated",
                task.Id,
                task.Status
            });
        }


        //delete task

        [HttpDelete("{taskId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var task = await _Context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound();

            var taskTitle = task.Title;
            var taskIdValue = task.Id;

            _Context.Tasks.Remove(task);

            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = taskIdValue,
                UserId = userId,
                Action = ActivityType.DeletedTask,
                Description = $"User deleted task '{taskTitle}'",
                CreatedAt = DateTime.UtcNow
            });

            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "Task deleted successfully",
                taskId = taskId
            });
        }





        //get the user tasks
        [HttpGet("user/{userId}/tasks")]
        [Authorize]
        public async Task<IActionResult>GetUsersTasks(string userId)
        {
            var UserId= User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(UserId==null) return Unauthorized();
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers.AnyAsync(p => p.UserId == userId);
                if (!isMember) return Forbid();
            }
            var UserTasks = await _Context.Tasks.Where(p =>p.AssignedUserId==userId).Select(t => new
            {
              t.Id,
              t.Title,
              t.Description,
              t.Status,
              t.priority,
              t.Deadline,
              t.ProjectId,
              AssignedUser = t.AssignedUser == null ? null : new
              {
                 t.AssignedUser.Id,
                 FullName = (t.AssignedUser.FName ?? "") + " " + (t.AssignedUser.LName ?? "")
                            }}).ToListAsync();
            return Ok(UserTasks);
           }




        //get the all task for Creator

        [HttpGet("TaskCreator/{createdBy}")]
        [Authorize]
        public async Task<IActionResult> GetTaskCreator(string createdBy)
        {
            var Userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Userid == null) return Unauthorized();
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers.AnyAsync(p => p.UserId == Userid);
                if (!isMember) return Forbid();
            }
            var UserTasks = await _Context.Tasks.Where(t => t.CreatedBy == createdBy).Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.priority,
                t.Deadline,
                t.ProjectId,
                AssignedUser = t.AssignedUser == null ? null : new
                {
                    t.AssignedUser.Id,
                    FullName = (t.AssignedUser.FName ?? "") + " " + (t.AssignedUser.LName ?? "")
                }
            }).ToListAsync();
            return Ok(UserTasks);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTasks(int? projectId,TasksStatus? status, Priority? priority)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            //  Start query
            var query = _Context.Tasks.AsQueryable();

            //  Filter by project   
            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId);

                if (!User.IsInRole("Admin"))
                {
                    var isMember = await _Context.ProjectUsers
                        .AnyAsync(p => p.ProjectId == projectId && p.UserId == userId);

                    if (!isMember)
                        return Forbid();
                }
            }

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status);
            }

            // Filter by priority
            if (priority.HasValue)
            {
                query = query.Where(t => t.priority == priority);
            }

            var tasks = await query
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.priority,
                    t.Deadline,
                    t.ProjectId,

                    AssignedUser = t.AssignedUser == null ? null : new
                    {
                        t.AssignedUser.Id,
                        FullName = (t.AssignedUser.FName ?? "") + " " + (t.AssignedUser.LName ?? "")
                    }
                })
                .ToListAsync();

            return Ok(tasks);
        }




















    }
}
