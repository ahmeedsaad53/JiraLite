using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using System.Security.Claims;

public interface ITaskRequestService
{
    Task<ServiceResponse<CreateTaskRequestResponseDTO>> CreateTaskRequest(TaskRequestDTO dto, ClaimsPrincipal user);

    Task<ServiceResponse<PaginatedResponse<TaskRequestItemDTO>>> GetRequests( RequestStatus? status, int? taskId, int page, int pageSize);

    Task<ServiceResponse<HandleRequestResponseDTO>> HandleRequest( HandleRequestDTO dto, int requestId, ClaimsPrincipal user);

    Task<ServiceResponse<List<MyRequestDTO>>> GetMyRequests(ClaimsPrincipal user);

    Task<ServiceResponse<string>> DeleteRequest(int requestId, ClaimsPrincipal user);
}
