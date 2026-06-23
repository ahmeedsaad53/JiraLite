using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLiteAPI.Service.DashBoardService
{
    public interface IDashBoardService
    {
        Task<object> GetAdminDashboard();
        Task<object> GetUserDashboard(ClaimsPrincipal User);
    }
}
