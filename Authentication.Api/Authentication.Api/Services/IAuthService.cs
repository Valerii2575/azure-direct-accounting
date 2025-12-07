using Authentication.Api.Models;

namespace Authentication.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> ExchangeCodeForTokensAsync(string code, string state);
        Task<AuthResponse> RefreshTokensAsync(string refreshToken);
        UserInfo ExtractUserInformToken(string idToken);
    }
}
