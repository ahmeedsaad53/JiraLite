using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using System.Security.Claims;

namespace JiraLiteAPI.Service.TaskSevice
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskResponseDTO>> AddNewTask(TaskDTO dto, ClaimsPrincipal user);

        Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetAllTasks(int projectId, ClaimsPrincipal user);

        Task<ServiceResponse<TaskResponseDTO>> GetTaskById(int projectId, int taskId, ClaimsPrincipal user);

        Task<ServiceResponse<string>> EditTaskStatus(int projectId, int taskId, ClaimsPrincipal user);

        Task<ServiceResponse<string>> DeleteTask(int taskId, ClaimsPrincipal user);

        Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetUsersTasks(string userId, ClaimsPrincipal user);

        Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetTaskCreator( ClaimsPrincipal user);

        Task<ServiceResponse<IEnumerable<TaskResponseDTO>>> GetTasks(
            int? projectId,
            TasksStatus? status,
            Priority? priority,
            ClaimsPrincipal user);
    }
}