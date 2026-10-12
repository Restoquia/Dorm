using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(DormApiClient apiClient, ISessionService session, ILogger<IndexModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        public List<RoomResponseDto> AvailableRooms { get; set; } = new();
        public List<DormitoryResponseDto> Dormitories { get; set; } = new();
        public DashboardStatsDto? Stats { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DormitoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoomType { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxRate { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var roomsTask = _apiClient.GetAvailableRoomsAsync(DormitoryId, RoomType, MaxRate);
                var dormsTask = _apiClient.GetDormitoriesAsync();
                var statsTask = _apiClient.GetDashboardStatsAsync();

                await Task.WhenAll(roomsTask, dormsTask, statsTask);

                AvailableRooms = await roomsTask;
                Dormitories = await dormsTask;
                Stats = await statsTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading landing page data.");
                StatusMessage = "Notice: Could not connect to API server. Ensure backend is running.";
                StatusMessageType = "warning";
            }
        }

        public async Task<IActionResult> OnPostReserveAsync(int roomId)
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                var reservation = await _apiClient.CreateReservationAsync(new ReservationCreateDto
                {
                    UserId = currentUser.UserId,
                    RoomId = roomId,
                    ReservationDate = DateTime.UtcNow,
                    Status = "Pending"
                });

                StatusMessage = $"Success! Your reservation request for Room #{roomId} has been submitted to management.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Reservation error: {ex.Message}";
                StatusMessageType = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create reservation.");
                StatusMessage = "Unexpected error submitting reservation.";
                StatusMessageType = "error";
            }

            return RedirectToPage(new { DormitoryId, RoomType, MaxRate });
        }
    }
}
