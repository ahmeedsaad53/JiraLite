using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using System.Security.Claims;

public interface IActivityLogService
{
    Task<ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>> GetAllLogs(int? taskId, int page, int pageSize);

    Task<ServiceResponse<PaginatedResponseDTO<ActivityLogResponseDTO>>> GetMyLogs(ClaimsPrincipal user, int? taskId, int page, int pageSize);
}
