using Humanizer;
using JiraLiteAPI.Data;
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
        [HttpPost("{taskId}")]
        [Authorize]
        public async Task<IActionResult> UploadAttachment(int taskId, IFormFile file)
        {
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            //  Validate file
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Limit file size (5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size exceeds 5 MB");

            //  Check task exists
            var task = await _Context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound("Task not found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == task.ProjectId && p.UserId == userId);

                if (!isMember)
                    return Forbid();
            }

            //  Prepare uploads folder
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            //  Generate unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

            //  Full file path
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //  Save file to server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //  Save to database
            var attachment = new Attachment
            {
                TaskId = taskId,
                FileName = file.FileName,
                FilePath = "/uploads/" + uniqueFileName,
                UploadedByUserId = userId,
                UploadAt = DateTime.Now
            };

            _Context.Attachments.Add(attachment);
            await _Context.SaveChangesAsync();

            //   Return response
            return Ok(new
            {
                message = "File uploaded successfully",
                attachmentId = attachment.Id,
                fileUrl = attachment.FilePath
            });
        }
    }
}
