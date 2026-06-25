using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using System.Security.Claims;

namespace JiraLiteAPI.Service.PService
{
    public interface IProjectService
    {
        Task<ServiceResponse<ProjectResponseDTO>> CreateProject(ProjectDTO dto, ClaimsPrincipal user);

        Task<ServiceResponse<IEnumerable<ProjectResponseDTO>>> GetAllProjects(ClaimsPrincipal user);

        Task<ServiceResponse<ProjectResponseDTO>> GetProjectById(int id, ClaimsPrincipal user);

        Task<ServiceResponse<string>> UpdateProject(int id, EditProjectDTO dto);

        Task<ServiceResponse<string>> UpdateProjectStatus(int id, UpdateProjectProgressDTO dto, ClaimsPrincipal user);

        Task<ServiceResponse<string>> DeleteProject(int id);
    }
}