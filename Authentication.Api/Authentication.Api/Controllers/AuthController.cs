using Authentication.Api.Models;
using Authentication.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("callback")]
        public async Task<ActionResult<AuthResponse>> HandleCallback([FromBody] AuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    return BadRequest("Authorization code is missing.");
                }

                var authResponse = await _authService.ExchangeCodeForTokensAsync(request.Code!, request.State!);
                return Ok(authResponse);
            }
            catch (Exception ex) 
            {
                return BadRequest($"Authentication failed: {ex.Message}");
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> RefreshTokens([FromBody] TokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest("Refresh token is required.");
                }
                var authResponse = await _authService.RefreshTokensAsync(request.RefreshToken);
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                return BadRequest($"Token refresh failed: {ex.Message}");
            }
        }

        [HttpGet("login-url")]
        public ActionResult<object> GetLoginUrl()
        {
            var clientId = HttpContext.RequestServices.GetService<IConfiguration>()["AzureAd:ClientId"];
            var tenantId= HttpContext.RequestServices.GetService<IConfiguration>()["AzureAd:TenantId"];
            var redirectIri = HttpContext.RequestServices.GetService<IConfiguration>()["AzureAd:RedirectUri"];
            var state =Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            var loginUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
                $"?client_id={clientId}" +
                $"&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(redirectIri!)}" +
                $"&response_mode=query" +
                $"&scope=openid%20profile%20email%20offline_access" +
                $"&state={state}" +
                $"&nonce={nonce}";

            return Ok(new { loginUrl, state, nonce });
        }
}
