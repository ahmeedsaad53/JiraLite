using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using System.Security.Claims;

public interface IProjectUserService
{
    Task<ServiceResponse<string>> AddUser(string userId, AddUserToProjectDTO dto);

    Task<ServiceResponse<string>> DeleteUserFromProject(int projectId, string userId);

    Task<ServiceResponse<IEnumerable<ProjectUserResponseDTO>>> GetAllUser(int projectId, ClaimsPrincipal user);
}