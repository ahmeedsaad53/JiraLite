using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace JiraLiteAPI.Service.CommentSernice
{
    public class CommentsService:ICommentService
    {
        private readonly AppDbContext _Context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _Context = context;
            _userManager = userManager;

        }

        public async  Task<object> MakeANewComment(CommentDTO commentDTO, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UnauthorizedAccessException();

         

            var task = await _Context.Tasks
                 .FirstOrDefaultAsync(t => t.Id == commentDTO.TaskId);
            if (task == null)
                throw new KeyNotFoundException("Task Not Found");

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId);

                if (!isMember)
                    throw new UnauthorizedAccessException();
            }

            var comment = new Comment
            {
                Content = commentDTO.Content,
                TaskId = commentDTO.TaskId,
                CreatedAt = DateTime.Now,
                UserId = userId
            };
            await _Context.Comments.AddAsync(comment);
            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = comment.TaskId,
                UserId = userId,
                Action = ActivityType.CommentAdded,
                Description = $"User added comment on task {task.Title}",
                CreatedAt = DateTime.UtcNow
            });

            await _Context.SaveChangesAsync();


            return  $"Comment added" ;
        }






       public async Task<object> GetComments(int? taskId, ClaimsPrincipal User, int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UnauthorizedAccessException();
            if (taskId.HasValue)
            {
                var task = await _Context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    throw new KeyNotFoundException("Task not found");

                if (!User.IsInRole("Admin"))
                {
                    var isMember = await _Context.ProjectUsers
                        .AnyAsync(p => p.ProjectId == task.ProjectId && p.UserId == userId);

                    if (!isMember)
                        throw new UnauthorizedAccessException();
                }
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

            return new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = comment
            };
        }


        public async Task<object> DeleteComment(int CommentId, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UnauthorizedAccessException();
            var comment = await _Context.Comments
               .Include(c => c.Task)
               .FirstOrDefaultAsync(t => t.Id == CommentId);
            if (comment == null) throw new KeyNotFoundException("Comment not found");


            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.ProjectUsers
                    .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == comment.Task.ProjectId);

                if (!isMember)
                    throw new UnauthorizedAccessException();
            }

            if (!User.IsInRole("Admin") && comment.UserId != userId)
                throw new UnauthorizedAccessException();
            var CommentVaule = comment.TaskId;
            var CommentContent = comment.Content;
            _Context.Comments.Remove(comment);

            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = CommentVaule,
                UserId = userId,
                Action = ActivityType.CommentDeleted,
                Description = $"User deleted comment: {CommentContent}",
                CreatedAt = DateTime.UtcNow
            });
            await _Context.SaveChangesAsync();

            return new
            {
                message = "Comment deleted successfully",
                Comment = comment
            };

        }

        public async   Task<object> GetMyComment(ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                throw new UnauthorizedAccessException();

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

            return comments;
        }





        public async Task<object> EditComment(int CommentId, EditCommentDTO dto, ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UnauthorizedAccessException();

            if (!User.IsInRole("Admin"))
            {
                var isMember = await _Context.Comments
                    .AnyAsync(pu => pu.UserId == userId && pu.Id == CommentId);

                if (!isMember)
                    throw new UnauthorizedAccessException();
            }
            var comment = await _Context.Comments.FirstOrDefaultAsync(t => t.Id == CommentId);
            if (comment == null) throw new KeyNotFoundException("Comment not found");


            if (!User.IsInRole("Admin") && comment.UserId != userId)
                throw new UnauthorizedAccessException();

            comment.Content = dto.Content;

            _Context.ActivityLogs.Add(new ActivityLog
            {
                TaskId = comment.TaskId,
                UserId = userId,
                Action = ActivityType.CommentUpdated,
                Description = $"User Edited the  comment Id{comment.Id} from task{comment.TaskId} ",
                CreatedAt = DateTime.UtcNow
            });
            await _Context.SaveChangesAsync();


            return new
            {
                message = "Comment Edited  successfully",
                Comment = comment
            };

        }































    }
}
