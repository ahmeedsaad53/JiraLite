using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace JiraLiteAPI.Service.CommentSernice
{
    public class CommentsService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        public async Task<ServiceResponse<string>> MakeANewComment(CommentDTO dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<string>.Fail("Unauthorized");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == dto.TaskId);

            if (task == null)
                return ServiceResponse<string>.Fail("Task not found");

            if (!user.IsInRole("Admin"))
            {
                var isMember = await _context.ProjectUsers
                    .AnyAsync(p => p.ProjectId == task.ProjectId && p.UserId == userId);

                if (!isMember)
                    return ServiceResponse<string>.Fail("Forbidden");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                TaskId = dto.TaskId,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Comments.Add(comment);

            _context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = comment.TaskId,
                UserId = userId,
                Action = ActivityType.CommentAdded,
                Description = $"User added comment on task {task.Title}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Created", "Comment added successfully");
        }



        public async Task<ServiceResponse<PaginatedResponseDTO<CommentResponseDTO>>> GetComments(int? taskId, ClaimsPrincipal user, int page, int pageSize)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<PaginatedResponseDTO<CommentResponseDTO>>.Fail("Unauthorized");

            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Comments.Include(c => c.User).Include(c => c.Task).AsQueryable();

            if (taskId.HasValue)
                query = query.Where(c => c.TaskId == taskId);

            var total = await query.CountAsync();

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentResponseDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserFullName = c.User == null ? "" : $"{c.User.FName} {c.User.LName}",
                    TaskId = c.TaskId,
                    TaskTitle = c.Task == null ? "" : c.Task.Title
                })
                .ToListAsync();

            var result = new PaginatedResponseDTO<CommentResponseDTO>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Data = comments
            };

            return ServiceResponse<PaginatedResponseDTO<CommentResponseDTO>>
                .SuccessResponse(result);
        }

        public async Task<ServiceResponse<string>> DeleteComment(int commentId, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<string>.Fail("Unauthorized");

            var comment = await _context.Comments
                .Include(c => c.Task)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return ServiceResponse<string>.Fail("Comment not found");

            if (!user.IsInRole("Admin") && comment.UserId != userId)
                return ServiceResponse<string>.Fail("Forbidden");
            var CommentVaule = comment.TaskId;
            var CommentContent = comment.Content;
            _context.Comments.Remove(comment);
            _context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = CommentVaule,
                UserId = userId,
                Action = ActivityType.CommentDeleted,
                Description = $"User deleted comment: {CommentContent}",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Deleted", "Comment deleted");
        }
        public async Task<ServiceResponse<IEnumerable<CommentResponseDTO>>> GetMyComment(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<IEnumerable<CommentResponseDTO>>.Fail("Unauthorized");

            var comments = await _context.Comments
                .Include(c => c.Task)
                .Where(c => c.UserId == userId)
                .Select(c => new CommentResponseDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    TaskId = c.TaskId,
                    TaskTitle = c.Task == null ? "" : c.Task.Title
                })
                .ToListAsync();

            return ServiceResponse<IEnumerable<CommentResponseDTO>>
                .SuccessResponse(comments);
        }



        public async Task<ServiceResponse<string>> EditComment(int commentId, EditCommentDTO dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<string>.Fail("Unauthorized");

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return ServiceResponse<string>.Fail("Comment not found");

            if (!user.IsInRole("Admin") && comment.UserId != userId)
                return ServiceResponse<string>.Fail("Forbidden");

            comment.Content = dto.Content;
            _context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = comment.TaskId,
                UserId = userId,
                Action = ActivityType.CommentUpdated,
                Description = $"User Edited the  comment Id{comment.Id} from task{comment.TaskId} ",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("Updated", "Comment updated");


        }



    }

    }































  
