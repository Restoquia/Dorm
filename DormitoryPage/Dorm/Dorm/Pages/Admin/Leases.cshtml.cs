using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Admin
{
    public class LeasesModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ILogger<LeasesModel> _logger;

        public LeasesModel(DormApiClient apiClient, ILogger<LeasesModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public List<LeaseResponseDto> Leases { get; set; } = new();
        public List<UserResponseDto> Tenants { get; set; } = new();
        public List<RoomResponseDto> AvailableRooms { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public LeaseCreateDto NewLease { get; set; } = new()
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(6)
        };

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Leases = await _apiClient.GetLeasesAsync(status: StatusFilter);
                Tenants = await _apiClient.GetUsersAsync();
                AvailableRooms = await _apiClient.GetRoomsAsync(status: "Available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load leases administration data.");
            }
        }

        public async Task<IActionResult> OnPostTerminateAsync(int leaseId)
        {
            try
            {
                await _apiClient.TerminateLeaseAsync(leaseId);
                StatusMessage = $"Lease #{leaseId} terminated successfully. Room slot released.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Failed to terminate lease: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateLeaseAsync()
        {
            if (NewLease.UserId <= 0 || NewLease.RoomId <= 0)
            {
                StatusMessage = "Please select both a valid tenant and room.";
                StatusMessageType = "error";
                return RedirectToPage();
            }

            if (NewLease.EndDate <= NewLease.StartDate)
            {
                StatusMessage = "End date must be after start date.";
                StatusMessageType = "error";
                return RedirectToPage();
            }

            try
            {
                await _apiClient.CreateLeaseAsync(NewLease);
                StatusMessage = "New active lease successfully initiated!";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Failed to create lease: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }
    }
}
