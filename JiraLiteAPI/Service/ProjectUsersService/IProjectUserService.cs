using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLiteAPI.Service.ProjectUsersService
{
    public interface IProjectUserService
    {
        Task<object> AddUser(string userId,  AddUserToProjectDTO dto);
        Task<object> DeleteUserFromProject(int projectId, string userId);
        Task<object> GetAllUser(int projectId, ClaimsPrincipal User);


    }
}
