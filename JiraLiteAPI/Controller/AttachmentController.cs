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
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        public AttachmentController(IAttachmentService attachmentService)
        {

            _attachmentService = attachmentService;
        }

       
        [HttpPost("{taskId}")]//add new file
        [Authorize]
        public async Task<IActionResult> UploadAttachment(int taskId, IFormFile file)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _attachmentService.UploadAttachment(taskId, file,User);
           
            return Ok(result);
        }





        [HttpGet]//get By Task
        public async Task<IActionResult> GetAllFills(int taskId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _attachmentService.GetAllFills(taskId, User);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _attachmentService.DownloadAttachment(id, User);
            var fileResult = (dynamic)result;

            return File(fileResult.FileBytes, fileResult.ContentType, fileResult.FileName);

        }



        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _attachmentService.DeleteAttachment(id, User);

            return Ok(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAttachments(int page = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _attachmentService.GetAllAttachments(page, pageSize);

            return Ok(result);
        }









    }
}
