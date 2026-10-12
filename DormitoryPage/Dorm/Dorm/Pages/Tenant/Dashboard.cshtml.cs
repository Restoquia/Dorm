using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Tenant
{
    public class DashboardModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(DormApiClient apiClient, ISessionService session, ILogger<DashboardModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        public UserDetailDto? UserProfile { get; set; }
        public List<LeaseResponseDto> ActiveLeases { get; set; } = new();
        public List<ReservationResponseDto> Reservations { get; set; } = new();
        public List<MaintenanceRequestResponseDto> MaintenanceRequests { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null) return RedirectToPage("/Account/Login");

            try
            {
                var profileTask = _apiClient.GetUserByIdAsync(currentUser.UserId);
                var leasesTask = _apiClient.GetLeasesAsync(userId: currentUser.UserId);
                var reservationsTask = _apiClient.GetReservationsAsync(userId: currentUser.UserId);
                var maintenanceTask = _apiClient.GetMaintenanceRequestsAsync(userId: currentUser.UserId);

                await Task.WhenAll(profileTask, leasesTask, reservationsTask, maintenanceTask);

                UserProfile = await profileTask;
                ActiveLeases = (await leasesTask).Where(l => l.Status == "Active").ToList();
                Reservations = await reservationsTask;
                MaintenanceRequests = await maintenanceTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tenant dashboard for user {UserId}", currentUser.UserId);
                ErrorMessage = "Unable to load your profile data. The API server may be offline.";
            }

            return Page();
        }
    }
}
