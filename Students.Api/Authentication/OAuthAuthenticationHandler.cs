using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Students.Application.Interfaces;

namespace Students.Api.Authentication
{
    public class OAuthAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IOAuthService _oauthService;

        public OAuthAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IOAuthService oauthService)
            : base(options, logger, encoder)
        {
            _oauthService = oauthService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if Authorization header exists
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization header");
            }

            var authorizationHeader = Request.Headers["Authorization"].ToString();
            
            // Check if it's a Bearer token
            if (string.IsNullOrEmpty(authorizationHeader) || 
                !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid Authorization header format");
            }

            try
            {
                // Validate token using OAuth Gateway
                var user = await _oauthService.ValidateTokenAsync(authorizationHeader);

                if (user == null)
                {
                    return AuthenticateResult.Fail("Invalid token");
                }

                // Create claims from authenticated user
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }.Concat(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error authenticating request");
                return AuthenticateResult.Fail($"Authentication error: {ex.Message}");
            }
        }
    }
}
