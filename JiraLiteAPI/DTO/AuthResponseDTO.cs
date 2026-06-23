namespace JiraLiteAPI.DTO.Auth
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}