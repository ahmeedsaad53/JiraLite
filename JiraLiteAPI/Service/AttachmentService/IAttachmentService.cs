using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using System.Security.Claims;

public interface IAttachmentService
{
    Task<ServiceResponse<string>> UploadAttachment(int taskId, IFormFile file, ClaimsPrincipal user);

    Task<ServiceResponse<IEnumerable<AttachmentResponseDTO>>> GetAllFiles(int taskId, ClaimsPrincipal user);

    Task<ServiceResponse<DownloadFileDTO>> DownloadAttachment(int id, ClaimsPrincipal user);

    Task<ServiceResponse<string>> DeleteAttachment(int id, ClaimsPrincipal user);
}