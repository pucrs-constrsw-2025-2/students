using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Students.Application.Interfaces;

namespace Students.Infrastructure.Services
{
    public class OAuthService : IOAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _oauthBaseUrl;

        public OAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            // Read OAuth configuration from environment variables or appsettings
            var protocol = configuration["OAuth:Protocol"] ?? "http";
            var host = configuration["OAuth:Host"] ?? "oauth";
            var port = configuration["OAuth:Port"] ?? "8080";
            
            _oauthBaseUrl = $"{protocol}://{host}:{port}";
        }

        public async Task<AuthenticatedUser?> ValidateTokenAsync(string token)
        {
            try
            {
                // Remove "Bearer " prefix if present
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring(7);
                }

                // Call OAuth Gateway's /me endpoint to validate the token
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_oauthBaseUrl}/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<OAuthUserResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userInfo == null)
                {
                    return null;
                }

                return new AuthenticatedUser
                {
                    Id = userInfo.Sub ?? string.Empty,
                    Username = userInfo.PreferredUsername ?? string.Empty,
                    Email = userInfo.Email ?? string.Empty,
                    Roles = userInfo.RealmAccess?.Roles ?? new List<string>()
                };
            }
            catch (Exception)
            {
                // Log the exception in production
                return null;
            }
        }

        // Internal classes to deserialize OAuth Gateway response
        private class OAuthUserResponse
        {
            public string? Sub { get; set; }
            public string? PreferredUsername { get; set; }
            public string? Email { get; set; }
            public RealmAccessInfo? RealmAccess { get; set; }
        }

        private class RealmAccessInfo
        {
            public List<string> Roles { get; set; } = new();
        }
    }
}
