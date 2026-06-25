using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using System.Security.Claims;

public interface IDashBoardService
{
    Task<ServiceResponse<AdminDashboardDTO>> GetAdminDashboard();

    Task<ServiceResponse<UserDashboardDTO>> GetUserDashboard(ClaimsPrincipal user);
}
