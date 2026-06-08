using JiraLiteAPI.Data;
using JiraLiteAPI.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public AccController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (registerDTO == null) return BadRequest("Invalid request");
            if (registerDTO.Password != registerDTO.ConfirmPassword) return BadRequest("Invalid request");
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
                await _Context.SaveChangesAsync();
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sgdsgd648d9f*/w43U4354t69ts8e22365fh"));
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
                expires: DateTime.Now.AddDays(7),
                signingCredentials: signingCredentials

                );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
                   .WriteToken(token),

                expiration = DateTime.Now.AddDays(7)
            });

            //https://localhost:7068/swagger/index.html
        }
        [HttpDelete]
        public async Task<IActionResult>DeleteUser(string UserId)
        {
            var user = await _Context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null) return BadRequest("User not Found");
             _Context.Users.Remove(user);
            await _Context.SaveChangesAsync();
            return Ok("User Deleted");
        }
        [HttpGet("Get All Users")]
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
        [HttpGet("{id:alpha}")]
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


        }
    }
