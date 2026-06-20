using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLiteAPI.Service.TaskSevice
{
    public interface ITaskService
    {
        Task<object> AddNewTask(TaskDTO taskDTO, ClaimsPrincipal user);
        Task<object> GetAllTasks(int projectId, ClaimsPrincipal user);
        Task<object> GetTasksById(int projectId, int taskId, ClaimsPrincipal user);
        Task<object> EditTaskStauts(int projectId, int taskId, ClaimsPrincipal user);
        Task<object> DeleteTask(int taskId, ClaimsPrincipal user);
        Task<object> GetUsersTasks(string userId, ClaimsPrincipal user);
        Task<object> GetTaskCreator(string createdBy, ClaimsPrincipal user);
        Task<object> GetTasks(int? projectId, TasksStatus? status, Priority? priority, ClaimsPrincipal user);
    }
}
