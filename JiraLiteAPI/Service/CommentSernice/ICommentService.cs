using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiraLiteAPI.Service.CommentSernice
{
    public interface ICommentService
    {
        Task<object> MakeANewComment(CommentDTO commentDTO, ClaimsPrincipal User);
        Task<object> GetComments(int? taskId, ClaimsPrincipal User, int page = 1, int pageSize = 10);
        Task<object> DeleteComment(int CommentId, ClaimsPrincipal User);
        Task<object> GetMyComment(ClaimsPrincipal User);
        Task<object> EditComment(int CommentId, EditCommentDTO dto , ClaimsPrincipal User);
          

    }
}
