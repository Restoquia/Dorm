using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(DormApiClient apiClient, ISessionService session, ILogger<RegisterModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        [BindProperty]
        public UserCreateDto Input { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                var user = await _apiClient.RegisterAsync(Input);
                if (user != null)
                {
                    _session.SetCurrentUser(user);
                    return RedirectToPage("/Tenant/Dashboard");
                }
                ErrorMessage = "Registration failed unexpectedly.";
            }
            catch (DormApiException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error.");
                ErrorMessage = "Unable to connect to API server.";
            }
            return Page();
        }
    }
}
