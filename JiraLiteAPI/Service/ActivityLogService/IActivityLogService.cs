using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLiteAPI.Service.ActivityLogService
{
    public interface IActivityLogService
    {
        Task<object> GetAllLogs(int? taskId, int page = 1, int pageSize = 10);
        Task<object> GetMyLogs(ClaimsPrincipal User, int? taskId, int page = 1, int pageSize = 10);
        
    }
}
