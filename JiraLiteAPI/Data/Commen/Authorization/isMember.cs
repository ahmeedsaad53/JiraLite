using JiraLiteAPI.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Services.Authorization
{
    public interface IAuthorizationServiceCustom
    {
        Task<bool> CanAccessProject(ClaimsPrincipal user, int projectId);
    }

    public class AuthorizationServiceCustom : IAuthorizationServiceCustom
    {
        private readonly AppDbContext _context;

        public AuthorizationServiceCustom(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanAccessProject(ClaimsPrincipal user, int projectId)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return false;

            //  Admin can access everything
            if (user.IsInRole("Admin"))
                return true;

            //  Check membership
            return await _context.ProjectUsers
                .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == projectId);
        }
    }
}