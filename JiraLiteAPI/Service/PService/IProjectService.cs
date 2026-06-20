using JiraLiteAPI.DTO;
using System.Security.Claims;

namespace JiraLiteAPI.Service.PService
{
    public interface IProjectService
    {
        Task<object> CreateProject(ProjectDTO dto, ClaimsPrincipal user);
        Task<object> GetAllProjects(ClaimsPrincipal user);
        Task<object> GetProjectById(int id, ClaimsPrincipal user);
        Task<string> UpdateProject(int id, EditProjectDTO dto);
        Task<string> UpdateProjectStatus(int id, UpdateProjectProgressDTO dto, ClaimsPrincipal user);
        Task<object> DeleteProject(int id);
    }
}
