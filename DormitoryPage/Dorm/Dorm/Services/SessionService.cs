using System.Text.Json;
using Dorm.Models;

namespace Dorm.Services
{
    public interface ISessionService
    {
        UserResponseDto? GetCurrentUser();
        void SetCurrentUser(UserResponseDto user);
        void ClearCurrentUser();
        bool IsLoggedIn { get; }
        bool IsAdmin { get; }
        bool IsTenant { get; }
    }

    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UserSessionKey = "CurrentDormUser";

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public UserResponseDto? GetCurrentUser()
        {
            var json = Session?.GetString(UserSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                // Return default demo user (John Doe, Tenant) so the visitor immediately experiences full functionality if they don't explicitly log in
                return new UserResponseDto
                {
                    UserId = 2,
                    Username = "john_doe",
                    FullName = "John Doe",
                    Email = "john.doe@email.com",
                    PhoneNumber = "0918-333-4444",
                    Role = "Tenant",
                    ActiveLeasesCount = 1
                };
            }

            try
            {
                return JsonSerializer.Deserialize<UserResponseDto>(json);
            }
            catch
            {
                return null;
            }
        }

        public void SetCurrentUser(UserResponseDto user)
        {
            var json = JsonSerializer.Serialize(user);
            Session?.SetString(UserSessionKey, json);
        }

        public void ClearCurrentUser()
        {
            Session?.Remove(UserSessionKey);
        }

        public bool IsLoggedIn => GetCurrentUser() != null;
        public bool IsAdmin => string.Equals(GetCurrentUser()?.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(GetCurrentUser()?.Role, "Landlord", StringComparison.OrdinalIgnoreCase);
        public bool IsTenant => string.Equals(GetCurrentUser()?.Role, "Tenant", StringComparison.OrdinalIgnoreCase);
    }
}
