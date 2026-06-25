using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Common;
using System.Security.Claims;

public interface ICommentService
{
    Task<ServiceResponse<string>> MakeANewComment(CommentDTO dto, ClaimsPrincipal user);

    Task<ServiceResponse<PaginatedResponseDTO<CommentResponseDTO>>> GetComments(int? taskId, ClaimsPrincipal user, int page, int pageSize);

    Task<ServiceResponse<string>> DeleteComment(int commentId, ClaimsPrincipal user);

    Task<ServiceResponse<IEnumerable<CommentResponseDTO>>> GetMyComment(ClaimsPrincipal user);

    Task<ServiceResponse<string>> EditComment(int commentId, EditCommentDTO dto, ClaimsPrincipal user);
}