using Humanizer;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JiraLiteAPI.Service.AttachmentService;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : BaseController
    {
        private readonly IAttachmentService _service;
        private readonly AppDbContext _context;

        public AttachmentController(IAttachmentService service,AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpPost("{taskId}")]
        [Authorize]
        public async Task<IActionResult> Upload(int taskId, IFormFile file)
        {
            var result = await _service.UploadAttachment(taskId, file, User);
            return HandleResponse(result);
        }

        [HttpGet("{taskId}")]
        [Authorize]
        public async Task<IActionResult> GetFiles(int taskId)
        {
            var result = await _service.GetAllFiles(taskId, User);
            return HandleResponse(result);
        }

        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _service.DownloadAttachment(id, User);

            if (!result.Success)
                return HandleResponse(result);

            return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAttachment(id, User);
            return HandleResponse(result);
        }
        [HttpGet("files/{fileName}")]
        [Authorize]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var attachment = await _context.Attachments
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.FilePath.Contains(fileName));

            if (attachment == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == attachment.Task.ProjectId && p.UserId == userId);

                if (!isMember)
                    return Forbid();
            }

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "../UploadedFiles",
                fileName
            );

            if (!System.IO.File.Exists(path))
                return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(path);

            return File(bytes, "application/octet-stream", fileName);
        }
    }





}

