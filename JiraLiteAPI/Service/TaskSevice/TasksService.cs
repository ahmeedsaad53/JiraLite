using JiraLiteAPI.Data;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using JiraLiteAPI.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace JiraLiteAPI.Service.TaskSevice
{
    public class TasksService: ITaskService
    {
        private readonly AppDbContext _Context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _Context = context;
            _userManager = userManager;

        }
        //add the new task 
        public async Task<object> AddNewTask(TaskDTO taskDTO, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();

            var project = await _Context.Projects
                           .FirstOrDefaultAsync(p => p.Id == taskDTO.ProjectId);

            if (project == null)
                throw new Exception("Project not found");
            //cheak if project are completed or cancelled
            if (project.Status == ProjectStatus.Completed ||
                project.Status == ProjectStatus.Cancelled)
                throw new Exception("Project already finished");
            //cheak if the deadline not  in furure
            if (taskDTO.Deadline < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new Exception("Deadline must be in the future");

            string? assignedUserId = null;
            //if the task is hard will make all this cheak to give it to the best user to do it is easy will let it the other users to take it 
            if (taskDTO.IsHard)
            {
                if (string.IsNullOrEmpty(taskDTO.AssignedUserId))//cheak if used id in dto is null
                    throw new Exception("AssignedUserId is required");

                var user = await _userManager.FindByIdAsync(taskDTO.AssignedUserId);//cheak if user id already in the db

                if (user == null)
                    throw new Exception("User not found");

                var isMember = await _Context.ProjectUsers // cheak if the user id already in the project 
                    .AnyAsync(pu => pu.ProjectId == taskDTO.ProjectId &&
                                    pu.UserId == taskDTO.AssignedUserId);

                if (!isMember)
                    throw new Exception("User not in project");

                assignedUserId = taskDTO.AssignedUserId;//give task for the user
            }

            var newTask = new WorkTask
            {
                Title = taskDTO.Title,
                Description = taskDTO.Description,
                Deadline = taskDTO.Deadline,
                CreatedBy = userId,
                Status = TasksStatus.ToDo,
                CreatedOn = DateTime.UtcNow,
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

            return new
            {
                message = "Task created successfully",
                taskId = newTask.Id
            };

        }




        //get project tasks
        public async Task<object> GetAllTasks(int projectId, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();
            var Project = await _Context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (Project == null) throw new Exception ("Project Not Found");


            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (!isMember)
                    throw new UnauthorizedAccessException();

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
                }
            })
            .ToListAsync();

            return new { projectTasks };
        }





        public async Task<object> GetTasksById(int projectId, int taskId, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new  UnauthorizedAccessException();

            var project = await _Context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) throw new Exception("Project Not Found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (!isMember)
                    throw new UnauthorizedAccessException();

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
               throw new Exception ("Task not found");

            return new { task };
        }











        //change the task status 
       public async Task<object> EditTaskStauts(int projectId, int taskId, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) throw new UnauthorizedAccessException();

            var project = await _Context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) throw new Exception("Project Not Found");

            var task = await _Context.Tasks.Where(pu => pu.ProjectId == projectId && pu.Id == taskId).FirstOrDefaultAsync();

            if (task == null) throw new  Exception("Not Found");

            if (!User.IsInRole("Admin") && task.AssignedUserId != userId) throw new UnauthorizedAccessException();
            // Only assigned user or admin

            if (task.Status == TasksStatus.Done)
                throw new Exception("Task already completed");

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


            return new
            {
                message = "Task status updated",
                task.Id,
                task.Status
            };
        }









        //delete task
       public async Task<object> DeleteTask(int taskId,ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new UnauthorizedAccessException();

            var task = await _Context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new Exception("Task Not Found");

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

            return  new
            {
                message = "Task deleted successfully",
                taskId = taskId
            };
        }



        //get task for user

      public async Task<object> GetUsersTasks(string userId, ClaimsPrincipal User)
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (UserId == null) throw new UnauthorizedAccessException ();
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers.AnyAsync(p => p.UserId == userId);
                if (!isMember)  throw new UnauthorizedAccessException () ;
            }
            var UserTasks = await _Context.Tasks.Where(p => p.AssignedUserId == userId).Select(t => new
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
            return new
            {
                UserTasks
            };
        }


        //Get Task Creator by one admin 
     public async  Task<object> GetTaskCreator(string createdBy,ClaimsPrincipal User)
        {
            var Userid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Userid == null) 
                throw new UnauthorizedAccessException();
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers.AnyAsync(p => p.UserId == Userid);
                if (!isMember) throw new UnauthorizedAccessException();
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
            return new { UserTasks };
        }







    public async  Task<object> GetTasks(int? projectId, TasksStatus? status, Priority? priority,ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
             throw new UnauthorizedAccessException();
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

                    if (!isMember) throw new UnauthorizedAccessException();

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

            return new { tasks };



        }





    }
}
