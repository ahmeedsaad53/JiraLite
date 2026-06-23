using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Auth;
using JiraLiteAPI.DTO.Common;

namespace JiraLiteAPI.Service.AccountService
{
    public interface IAccountService
    {
        Task<ServiceResponse<string>> Register(RegisterDTO dto);
        Task<ServiceResponse<AuthResponseDTO>> Login(LoginDTO dto);
        Task<ServiceResponse<object>> DeleteUser(string id);
        Task<ServiceResponse<List<object>>> GetAllUser();
        Task<ServiceResponse<object>> GetById(string id);
        Task<ServiceResponse<string>> AssignRole(AssignRoleDTO dto);
    }
}