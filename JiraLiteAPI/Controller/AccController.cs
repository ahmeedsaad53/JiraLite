using JiraLiteAPI.Data;
using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IConfiguration _config;


        public AccController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _Context = context;
            _config = config; 
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

  

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    Email = registerDTO.Email,
                    FName = registerDTO.FName,
                    UserName = registerDTO.Email,
                    LName = registerDTO.LName,
                    PhoneNumber = registerDTO.phoneNumber
                };
                IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
                if (!result.Succeeded) return BadRequest(result.Errors);
                await _userManager.AddToRoleAsync(user, "User");
                return Ok("User created successfully");
            }
            return BadRequest("Invalid request");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            
            if (!ModelState.IsValid) return BadRequest("Invalid request");
            ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null) return BadRequest("Invalid UserName or Password");
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
            if (!isPasswordValid) return BadRequest("Invalid UserName or Password");
            List <Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FName + " " + user.LName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var key = _config["JwtSettings:Key"];
            if (string.IsNullOrEmpty(key))
                throw new Exception("JWT key is missing");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            JwtSecurityToken token = new JwtSecurityToken(
               issuer: "http://localhost:5009",
               audience: "http://localhost:5009",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: signingCredentials

                );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
                   .WriteToken(token),

                expiration = DateTime.UtcNow.AddDays(7)
            });

        }
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>DeleteUser(string userId)
        {
            var user = await _Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return BadRequest("User not Found");
            await _userManager.DeleteAsync(user);

            return Ok(new
            {
                message = "User deleted successfully",
                userId = userId
            });
        }
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUser()
        {
            var Users=await _Context.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.PhoneNumber,
                u.Email

            }).ToListAsync();
            return Ok(Users);
               
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult>GetById(string id)
        {
            var user= await _Context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return BadRequest();
            else return Ok(new
            { 
                user.UserName,
                user.PhoneNumber,
                user.Email,
                user.Id
            });
        }
        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(AssignRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return NotFound("User not found");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest("Role does not exist");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Role assigned successfully");
        }

    }
    }
