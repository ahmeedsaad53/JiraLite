using JiraLiteAPI.DTO;
using JiraLiteAPI.DTO.Auth;
using JiraLiteAPI.DTO.Common;

public interface IAccountService
{
    Task<ServiceResponse<string>> Register(RegisterDTO dto);

    Task<ServiceResponse<AuthResponseDTO>> Login(LoginDTO dto);

    Task<ServiceResponse<string>> DeleteUser(string id);

    Task<ServiceResponse<IEnumerable<UserResponseDTO>>> GetAllUser();

    Task<ServiceResponse<UserResponseDTO>> GetById(string id);

    Task<ServiceResponse<string>> AssignRole(AssignRoleDTO dto);
}