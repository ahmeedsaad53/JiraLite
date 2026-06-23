using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLiteAPI.Service.AttachmentService
{
    public interface IAttachmentService
    {
        Task<object> UploadAttachment(int taskId, IFormFile file,ClaimsPrincipal User);
        Task<object> GetAllFills(int taskId, ClaimsPrincipal User);
        Task<object> DownloadAttachment(int id, ClaimsPrincipal User);
        Task<object> DeleteAttachment(int id, ClaimsPrincipal User);
        Task<object> GetAllAttachments(int page = 1, int pageSize = 10);
    }
}
