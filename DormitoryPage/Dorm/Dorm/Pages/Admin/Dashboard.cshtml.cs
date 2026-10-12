using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(DormApiClient apiClient, ILogger<DashboardModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public DashboardStatsDto? Stats { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Stats = await _apiClient.GetDashboardStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load admin dashboard stats.");
                ErrorMessage = "Could not connect to API server. Please ensure the backend is running on localhost:5210.";
            }
        }
    }
}
