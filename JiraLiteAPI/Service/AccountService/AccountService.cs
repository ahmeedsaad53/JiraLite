using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Auth;
using JiraLiteAPI.DTO.Common;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JiraLiteAPI.Service.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AccountService(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              AppDbContext context,
                              IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
        }

        public async Task<ServiceResponse<string>> Register(RegisterDTO dto)
        {
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FName = dto.FName,
                LName = dto.LName,
                PhoneNumber = dto.phoneNumber
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return ServiceResponse<string>.Fail(
                    "Registration failed",
                    result.Errors.Select(e => e.Description).ToList()
                );

            await _userManager.AddToRoleAsync(user, "User");

            return ServiceResponse<string>.SuccessResponse("User created successfully");
        }

        public async Task<ServiceResponse<AuthResponseDTO>> Login(LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return ServiceResponse<AuthResponseDTO>.Fail("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                return ServiceResponse<AuthResponseDTO>.Fail("Invalid credentials");

            var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FName} {user.LName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = _config["JwtSettings:Key"]
                      ?? throw new Exception("JWT key missing");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var token = new JwtSecurityToken(
                issuer: "http://localhost:5009",
                audience: "http://localhost:5009",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            var response = new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };

            return ServiceResponse<AuthResponseDTO>.SuccessResponse(response);
        }

        public async Task<ServiceResponse<object>> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return ServiceResponse<object>.Fail("User not found");

            await _userManager.DeleteAsync(user);

            return ServiceResponse<object>.SuccessResponse(new { id }, "Deleted successfully");
        }

        public async Task<ServiceResponse<List<object>>> GetAllUser()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.UserName,
                    u.PhoneNumber
                }).ToListAsync<object>();

            return ServiceResponse<List<object>>.SuccessResponse(users);
        }

        public async Task<ServiceResponse<object>> GetById(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return ServiceResponse<object>.Fail("User not found");

            return ServiceResponse<object>.SuccessResponse(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber
            });
        }

        public async Task<ServiceResponse<string>> AssignRole(AssignRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return ServiceResponse<string>.Fail("User not found");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return ServiceResponse<string>.Fail("Role does not exist");

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (!result.Succeeded)
                return ServiceResponse<string>.Fail(
                    "Failed to assign role",
                    result.Errors.Select(e => e.Description).ToList()
                );

            return ServiceResponse<string>.SuccessResponse("Role assigned");
        }
    }
}