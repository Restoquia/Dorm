using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Admin
{
    public class ReservationsModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ILogger<ReservationsModel> _logger;

        public ReservationsModel(DormApiClient apiClient, ILogger<ReservationsModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public List<ReservationResponseDto> Reservations { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Reservations = await _apiClient.GetReservationsAsync(status: StatusFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reservations.");
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int reservationId)
        {
            try
            {
                await _apiClient.ApproveReservationAsync(reservationId);
                StatusMessage = $"Reservation #{reservationId} approved successfully!";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int reservationId)
        {
            try
            {
                await _apiClient.CancelReservationAsync(reservationId);
                StatusMessage = $"Reservation #{reservationId} cancelled.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostConvertAsync(int reservationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                await _apiClient.ConvertReservationToLeaseAsync(reservationId, new ReservationConvertToLeaseDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                });
                StatusMessage = $"Reservation #{reservationId} converted to active lease! Tenant is now checked in.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Error converting to lease: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }
    }
}
