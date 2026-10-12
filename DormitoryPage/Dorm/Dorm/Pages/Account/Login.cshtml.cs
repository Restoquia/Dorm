using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(DormApiClient apiClient, ISessionService session, ILogger<LoginModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        [BindProperty]
        public UserLoginDto Input { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var user = await _apiClient.LoginAsync(Input);
                if (user != null)
                {
                    _session.SetCurrentUser(user);
                    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(user.Role, "Landlord", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToPage("/Admin/Dashboard");
                    }
                    return RedirectToPage("/Tenant/Dashboard");
                }

                ErrorMessage = "Invalid username or password.";
            }
            catch (DormApiException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login exception.");
                ErrorMessage = "Unable to connect to authentication server.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostQuickSwitchAsync(string username)
        {
            try
            {
                string password = username == "admin_maria" ? "admin123" : "tenant123";
                var user = await _apiClient.LoginAsync(new UserLoginDto
                {
                    Username = username,
                    Password = password
                });

                if (user != null)
                {
                    _session.SetCurrentUser(user);
                    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(user.Role, "Landlord", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToPage("/Admin/Dashboard");
                    }
                    return RedirectToPage("/Tenant/Dashboard");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Quick switch login failed for user {Username}", username);
            }

            return RedirectToPage("/Index");
        }

        public IActionResult OnGetLogout()
        {
            _session.ClearCurrentUser();
            return RedirectToPage("/Index");
        }
    }
}
