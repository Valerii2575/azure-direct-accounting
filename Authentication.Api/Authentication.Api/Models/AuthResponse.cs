
namespace Authentication.Api.Models
{
    public class AuthResponse
    {
        public string? AccessToken { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public UserInfo? User { get; set; } 
    }
}
