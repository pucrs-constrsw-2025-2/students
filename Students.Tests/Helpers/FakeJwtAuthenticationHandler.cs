using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Students.Tests.Helpers
{
    /// <summary>
    /// Fake JWT authentication handler for integration tests.
    /// Bypasses Keycloak and always returns successful authentication.
    /// </summary>
    public class FakeJwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FakeJwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Create fake claims for authenticated test user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "test-user@pucrs.br"),
                new Claim(ClaimTypes.Email, "test-user@pucrs.br"),
                new Claim(ClaimTypes.Role, "administrator"),
                new Claim("preferred_username", "test-user")
            };

            var identity = new ClaimsIdentity(claims, "FakeScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "FakeScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
