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
            var protocol = Environment.GetEnvironmentVariable("OAUTH_INTERNAL_PROTOCOL") 
                ?? configuration["OAuth:Protocol"] ?? "http";
            var host = Environment.GetEnvironmentVariable("OAUTH_INTERNAL_HOST") 
                ?? configuration["OAuth:Host"] ?? "oauth";
            var port = Environment.GetEnvironmentVariable("OAUTH_INTERNAL_API_PORT") 
                ?? configuration["OAuth:Port"] ?? "8000";
            
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

                // Call OAuth Gateway's /api/v1/validate endpoint to validate the token
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_oauthBaseUrl}/api/v1/validate");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var validateResponse = JsonSerializer.Deserialize<ValidateResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (validateResponse == null || !validateResponse.Active)
                {
                    return null;
                }

                // Extract user information from the validate response
                var roles = new List<string>();
                if (validateResponse.RealmAccess != null && validateResponse.RealmAccess.Roles != null)
                {
                    roles.AddRange(validateResponse.RealmAccess.Roles);
                }
                
                return new AuthenticatedUser
                {
                    Id = validateResponse.Sub ?? string.Empty,
                    Username = validateResponse.Username ?? validateResponse.PreferredUsername ?? string.Empty,
                    Email = validateResponse.Email ?? string.Empty,
                    Roles = roles
                };
            }
            catch (Exception)
            {
                // Log the exception in production
                return null;
            }
        }

        // Internal classes to deserialize OAuth Gateway validate response
        private class ValidateResponse
        {
            public bool Active { get; set; }
            public string? Sub { get; set; }
            public string? Username { get; set; }
            public string? PreferredUsername { get; set; }
            public string? Email { get; set; }
            public RealmAccessInfo? RealmAccess { get; set; }
        }

        private class RealmAccessInfo
        {
            public List<string>? Roles { get; set; }
        }
    }
}
