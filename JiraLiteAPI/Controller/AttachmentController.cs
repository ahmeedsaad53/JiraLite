using Humanizer;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public AttachmentController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }
        [HttpPost("{taskId}")]//add new file
        [Authorize]
        public async Task<IActionResult> UploadAttachment(int taskId, IFormFile file)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            //  Validate file
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File too large");

            var allowedExtensions = new[] { ".jpg", ".png", ".pdf", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("File type not allowed");

            //  Check task
            var task = await _Context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return NotFound("Task not found");

            //  Check membership
            
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == task.ProjectId && p.UserId == userId);

                if (!isMember)
                    return Forbid();
            

            //  Prepare folder
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            //  Safe + unique file name
            var safeFileName = Path.GetFileName(file.FileName);   
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName;

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //  Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //  Save in DB
            var attachment = new Attachment
            {
                TaskId = taskId,
                FileName = safeFileName,
                FilePath = "/uploads/" + uniqueFileName,
                UploadedByUserId = userId,
                UploadAt = DateTime.UtcNow
            };

             _Context.Attachments.Add(attachment);
            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = task.Id,
                UserId = userId,
                Action = ActivityType.UploadFile,
                Description = $"User {userId} Upload File To {task.Title} ",
                CreatedAt = DateTime.UtcNow
            });
            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "File uploaded successfully",
                attachmentId = attachment.Id,
                fileUrl = attachment.FilePath
            });
        }





        [HttpGet]//get By Task
        public async Task<IActionResult> GetAllFills(int taskId)
        {
            var userId=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            var task= _Context.Tasks.FirstOrDefault(t=>t.Id == taskId);
            if (task == null)
                return NotFound("Task Not Found");
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers.AnyAsync(p => p.ProjectId == task.ProjectId && p.UserId == userId);
                if (!isMember) return Forbid();
            }
            var attachments= await _Context.Attachments.Where(a=>a.TaskId == taskId).Select(a => new
            {
                a.Id,
                a.FileName,
                a.FilePath,
                a.UploadAt,
                UploadBy = a.UploadedByUser == null ? null : new
                {
                    a.UploadedByUser.Id,
                    FullName=(a.UploadedByUser.FName ??"") + " " +(a.UploadedByUser.LName??"")
                }
            }).ToListAsync();
            return Ok(attachments);         
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var attachment = await _Context.Attachments.Where(a=>a.Id==id).FirstOrDefaultAsync();
               

            if (attachment == null)
                return NotFound("Attachment not found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(p =>
                        p.ProjectId == attachment.Task.ProjectId &&
                        p.UserId == userId);

                if (!isMember)
                    return Forbid();
            }

            //  Build full file path
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                attachment.FilePath.TrimStart('/')
            );

            //  Check file exists on disk
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server");

            //  Get file bytes
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            //  Return file
            return File(fileBytes, "application/octet-stream", attachment.FileName);
        }



        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var attachment = await _Context.Attachments.Where(a=>a.Id== id).FirstOrDefaultAsync();
               

            if (attachment == null)
                return NotFound("Attachment not found");
            var task = await _Context.Tasks.FirstOrDefaultAsync(t => t.Id == attachment.TaskId);
            if (task == null) return NotFound("Task Not Found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(p =>
                        p.ProjectId == attachment.Task.ProjectId &&
                        p.UserId == userId);

                if (!isMember)
                    return Forbid();

                //  Only uploader or admin can delete
                if (attachment.UploadedByUserId != userId)
                    return Forbid();
            }

            // Delete file from disk
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                attachment.FilePath.TrimStart('/')
            );

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _Context.Attachments.Remove(attachment);
            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = task.Id,
                UserId = userId,
                Action = ActivityType.DeletedFile,
                Description = $"User {userId} Delete File From {task.Title} ",
                CreatedAt = DateTime.UtcNow
            });
            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "Attachment deleted successfully",
                attachmentId = id
            });
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAttachments(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _Context.Attachments.AsQueryable();

            var totalCount = await query.CountAsync();

            var attachments = await query
                .OrderByDescending(a => a.UploadAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.FileName,
                    a.FilePath,
                    a.UploadAt,

                    Task = a.Task == null ? null : new
                    {
                        a.Task.Id,
                        a.Task.Title
                    },

                    UploadedBy = a.UploadedByUser == null ? null : new
                    {
                        a.UploadedByUser.Id,
                        FullName = (a.UploadedByUser.FName ?? "") + " " + (a.UploadedByUser.LName ?? "")
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = attachments
            });
        }









    }
}
