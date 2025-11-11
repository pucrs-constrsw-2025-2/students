using System.Threading.Tasks;

namespace Students.Application.Interfaces
{
    public interface IOAuthService
    {
        Task<AuthenticatedUser?> ValidateTokenAsync(string token);
    }

    public class AuthenticatedUser
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
