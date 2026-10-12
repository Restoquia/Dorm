using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Rooms
{
    public class DetailsModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ISessionService _session;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(DormApiClient apiClient, ISessionService session, ILogger<DetailsModel> logger)
        {
            _apiClient = apiClient;
            _session = session;
            _logger = logger;
        }

        public RoomResponseDto? Room { get; set; }
        public DormitoryDetailDto? Dormitory { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Room = await _apiClient.GetRoomByIdAsync(Id);
                if (Room == null)
                {
                    return RedirectToPage("/Index");
                }

                if (Room.DormitoryId.HasValue)
                {
                    Dormitory = await _apiClient.GetDormitoryByIdAsync(Room.DormitoryId.Value);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load room details for ID {RoomId}", Id);
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostReserveAsync()
        {
            var currentUser = _session.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                await _apiClient.CreateReservationAsync(new ReservationCreateDto
                {
                    UserId = currentUser.UserId,
                    RoomId = Id,
                    ReservationDate = DateTime.UtcNow,
                    Status = "Pending"
                });

                StatusMessage = "Reservation successfully booked! The dormitory administrator will verify your request.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Unable to book: {ex.Message}";
                StatusMessageType = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation for Room {Id}", Id);
                StatusMessage = "An unexpected error occurred while booking.";
                StatusMessageType = "error";
            }

            return RedirectToPage(new { id = Id });
        }
    }
}
