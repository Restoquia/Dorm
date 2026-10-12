using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Tenant
{
    public class MaintenanceModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<MaintenanceModel> _logger;

        public MaintenanceModel(DormApiClient apiClient, ISessionService session, ILogger<MaintenanceModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        public List<MaintenanceRequestResponseDto> MyRequests { get; set; } = new();
        public List<LeaseResponseDto> ActiveLeases { get; set; } = new();

        [BindProperty]
        public MaintenanceRequestCreateDto Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null) return RedirectToPage("/Account/Login");

            try
            {
                var reqTask = _apiClient.GetMaintenanceRequestsAsync(userId: currentUser.UserId);
                var leaseTask = _apiClient.GetLeasesAsync(userId: currentUser.UserId);
                await Task.WhenAll(reqTask, leaseTask);

                MyRequests = await reqTask;
                ActiveLeases = (await leaseTask).Where(l => l.Status == "Active").ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load maintenance page.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null) return RedirectToPage("/Account/Login");

            Input.UserId = currentUser.UserId;
            Input.Status = "Pending";

            try
            {
                await _apiClient.CreateMaintenanceRequestAsync(Input);
                StatusMessage = "Maintenance request submitted successfully! The building administrator has been notified.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusMessageType = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting maintenance request.");
                StatusMessage = "Failed to submit maintenance request.";
                StatusMessageType = "error";
            }

            return RedirectToPage();
        }
    }
}
