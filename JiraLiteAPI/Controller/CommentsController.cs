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
    public class CommentsController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public CommentsController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult>MakeANewComment(CommentDTO commentDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);

            var task = await _Context.Tasks
                 .FirstOrDefaultAsync(t => t.Id == commentDTO.TaskId);
            if (task == null)
                return NotFound("Task not found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId);

                if (!isMember)
                    return Forbid();
            }
           
            var comment = new Comment
            {
                Content = commentDTO.Content,
                TaskId = commentDTO.TaskId,
                CreatedAt = DateTime.Now,
                UserId = userId
            };
            await _Context.Comments.AddAsync(comment);
            await _Context.SaveChangesAsync();


            return Ok("Comment added");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetComments( int? taskId, int page = 1, int pageSize = 10)
        {

            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            var task = await _Context.Tasks
               .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return NotFound("Task not found");
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId);

                if (!isMember)
                    return Forbid();
            }

            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _Context.Comments.AsQueryable();

            
            // Filter by task
            if (taskId.HasValue)
                query = query.Where(r => r.TaskId == taskId);

            // Total count
            var totalCount = await query.CountAsync();

            //  Pagination + sorting
            var comment = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Content,
                    r.CreatedAt,
                    r.UserId,
                    User = r.User == null ? null : new
                    {
                        FullName = (r.User.FName ?? "") + " " + (r.User.LName ?? "")
                    },

                    Task = r.Task == null ? null : new
                    {
                        r.Task.Id,
                        r.Task.Title
                    }

                    
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = comment
            });
        }
        [HttpDelete("{CommentId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int CommentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
    
            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu=> pu.UserId == userId);

                if (!isMember)
                    return Forbid();
            }
            var comment = await _Context.Comments.FirstOrDefaultAsync(t => t.Id == CommentId);
            if (comment == null) return NotFound();
            if (!User.IsInRole("Admin") && comment.UserId != userId)
                return Forbid();
            _Context.Comments.Remove(comment);
            await _Context.SaveChangesAsync();
            return Ok(new
            {
                message = "Comment deleted successfully",
                Comment = comment
            });
        
        }
        [HttpGet("myComment")]
        [Authorize]
        public async Task<IActionResult> GetMyComment()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var comments = await _Context.Comments

               .Where(r => r.UserId == userId)
               .Select(r => new
               {
                   r.Id,
                   r.Content,
                   r.CreatedAt,
                   r.TaskId,
                  

                   Task = r.Task == null ? null : new
                   {
                       r.Task.Id,
                       Title = r.Task.Title,
                       Status = r.Task.Status,
                       Deadline = r.Task.Deadline
                   }
               })
                .ToListAsync();

            return Ok(comments);
        }


        [HttpPatch("{CommentId:int}")]
        [Authorize]
        public async Task<IActionResult> EditComment(int CommentId,EditCommentDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.Comments
                    .AnyAsync(pu => pu.UserId == userId&&pu.Id==CommentId);

                if (!isMember)
                    return Forbid();
            }
            var comment = await _Context.Comments.FirstOrDefaultAsync(t => t.Id == CommentId);
            if (comment == null) return NotFound();

            if (!User.IsInRole("Admin") && comment.UserId != userId)
                return Forbid();

            comment.Content = dto.Content;
            await _Context.SaveChangesAsync();
            return Ok(new
            {
                message = "Comment Edited  successfully",
                Comment = comment
            });

        }














    }
}
