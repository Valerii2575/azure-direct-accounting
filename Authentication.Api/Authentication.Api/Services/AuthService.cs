using Authentication.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Authentication.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _redirectUri;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _clientId = _configuration["OAuth:ClientId"] ?? string.Empty;
            _clientSecret = _configuration["OAuth:ClientSecret"] ?? string.Empty;
            _tenantId = _configuration["OAuth:TenantId"] ?? string.Empty;
            _redirectUri = _configuration["OAuth:RedirectUri"] ?? string.Empty;
        }

        public async Task<AuthResponse> ExchangeCodeForTokensAsync(string code, string state)
        {
            var tokenEndpoint = $"https://login.microsoftline.com/{_tenantId}/oauth/v2.0/token";
            var requestBody = new List<KeyValuePair<string, string>>
            {
                new("client_id", _clientId),
                new("client_secret", _clientSecret),
                new("grant_type", "authorization_code"),
                new("redirect_uri", _redirectUri),
                new("scope", "openid offline_avvess profile email")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token exchange failed: {responseContent}");
            }

            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

            var accessToken = tokenResponse?["access_token"]?.ToString() ?? string.Empty;
            var idToken = tokenResponse?["id_token"]?.ToString() ?? string.Empty;
            var refreshToken = tokenResponse?["refresh_token"]?.ToString() ?? string.Empty;
            var expiresIn = int.Parse(tokenResponse?["expires_in"]?.ToString() ?? "0");

            var userInfo = ExtractUserInformToken(idToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                IdToken = idToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                User = userInfo
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var tokenEndpoint = $"https://login.microsoftline.com/{_tenantId}/oauth/v2.0/token";

            var requestBody = new List<KeyValuePair<string, string>>
            {
                new("client_id", _clientId),
                new("client_secret", _clientSecret),
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken),
                new("scope", "openid offline_avvess profile email")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token refresh failed: {responseContent}");
            }

            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
            var accessToken = tokenResponse?["access_token"]?.ToString() ?? string.Empty;
            var idToken = tokenResponse?["id_token"]?.ToString() ?? string.Empty;
            var newRefreshToken = tokenResponse?["refresh_token"]?.ToString() ?? string.Empty;
            var expiresIn = int.Parse(tokenResponse?["expires_in"]?.ToString() ?? "0");
            var userInfo = ExtractUserInformToken(idToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                IdToken = idToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn,
                User = userInfo
            };
        }

        public UserInfo ExtractUserInformToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);

            return new UserInfo
            {
                Id = token.Claims.FirstOrDefault(c => c.Type == "oid")?.Value,
                Name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                Email = token.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == "preferred_username")?.Value,
                PreferredUsername = token.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
            };
        }
    }
}
