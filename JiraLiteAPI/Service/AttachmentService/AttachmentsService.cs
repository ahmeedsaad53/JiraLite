  using Humanizer;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace JiraLiteAPI.Service.AttachmentService
{
    public class AttachmentsService:IAttachmentService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        public AttachmentsService(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ServiceResponse<string>> UploadAttachment(int taskId, IFormFile file, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<string>.Fail("Unauthorized");

            if (file == null || file.Length == 0)
                return ServiceResponse<string>.Fail("No file uploaded");

            if (file.Length > 5 * 1024 * 1024)
                return ServiceResponse<string>.Fail("File too large");

            var allowed = new[] { ".jpg", ".png", ".pdf", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(ext))
                return ServiceResponse<string>.Fail("File type not allowed");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return ServiceResponse<string>.Fail("Task not found");

            var isMember = await _context.ProjectUsers
                .AnyAsync(p => p.ProjectId == task.ProjectId && p.UserId == userId);

            if (!isMember)
                return ServiceResponse<string>.Fail("Forbidden");

            try  {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "../UploadedFiles");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var uniqueName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadPath, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"files/{uniqueName}";

                var attachment = new Attachment
                {
                    TaskId = taskId,
                    FileName = file.FileName,
                    FilePath = fileUrl,
                    UploadedByUserId = userId,
                    UploadAt = DateTime.UtcNow
                };

                _context.Attachments.Add(attachment);
                _context.ActivityLogs.Add(new ActivityLog
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Action = ActivityType.UploadFile,
                    Description = $"User {userId} Upload File To {task.Title} ",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return ServiceResponse<string>.SuccessResponse(fileUrl, "File uploaded successfully");
                }
            catch (Exception ex)
                {
                return ServiceResponse<string>.Fail("Upload failed", new List<string> { ex.Message });
                }
        }


       


        
        public async Task<ServiceResponse<IEnumerable<AttachmentResponseDTO>>> GetAllFiles(int taskId, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<IEnumerable<AttachmentResponseDTO>>.Fail("Unauthorized");

            var files = await _context.Attachments
                .Include(a => a.UploadedByUser)
                .Where(a => a.TaskId == taskId)
                .Select(a => new AttachmentResponseDTO
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileUrl = a.FilePath,
                    UploadAt = a.UploadAt,
                    UploadedByName = a.UploadedByUser == null
                        ? ""
                        : a.UploadedByUser.FName + " " + a.UploadedByUser.LName
                })
                .ToListAsync();

            return ServiceResponse<IEnumerable<AttachmentResponseDTO>>
                .SuccessResponse(files);
        }
        public async Task<ServiceResponse<DownloadFileDTO>> DownloadAttachment(int id, ClaimsPrincipal user)
        {
            var attachment = await _context.Attachments.FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null)
                return ServiceResponse<DownloadFileDTO>.Fail("Not found");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "../UploadedFiles");

            var fullPath = Path.Combine(uploadPath, attachment.FilePath.Replace("files/", ""));

            if (!File.Exists(fullPath))
                return ServiceResponse<DownloadFileDTO>.Fail("File missing");

            var bytes = await File.ReadAllBytesAsync(fullPath);

            return ServiceResponse<DownloadFileDTO>.SuccessResponse(new DownloadFileDTO
            {
                FileBytes = bytes,
                FileName = attachment.FileName,
                ContentType = "application/octet-stream"
            });
        }
        public async Task<ServiceResponse<string>> DeleteAttachment(int id, ClaimsPrincipal user)
        {
            var attachment = await _context.Attachments.FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null)
                return ServiceResponse<string>.Fail("Not found");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "../UploadedFiles");
            var fullPath = Path.Combine(uploadPath, attachment.FilePath.Replace("files/", ""));

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Deleted", "File removed");
        }































    }
}
