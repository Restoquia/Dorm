using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dorm.Models;
using Dorm.Services;

namespace Dorm.Pages.Admin
{
    public class RoomsModel : PageModel
    {
        private readonly DormApiClient _apiClient;
        private readonly ILogger<RoomsModel> _logger;

        public RoomsModel(DormApiClient apiClient, ILogger<RoomsModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public List<RoomResponseDto> Rooms { get; set; } = new();
        public List<DormitoryResponseDto> Dormitories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? DormitoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty]
        public RoomCreateDto NewRoom { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusMessageType { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var roomsTask = _apiClient.GetRoomsAsync(dormId: DormitoryId, status: Status);
                var dormsTask = _apiClient.GetDormitoriesAsync();
                await Task.WhenAll(roomsTask, dormsTask);
                Rooms = await roomsTask;
                Dormitories = await dormsTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rooms.");
                StatusMessage = "Failed to load rooms from API.";
                StatusMessageType = "error";
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                await _apiClient.CreateRoomAsync(NewRoom);
                StatusMessage = $"Room '{NewRoom.RoomNumber}' has been created successfully!";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Error creating room: {ex.Message}";
                StatusMessageType = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room.");
                StatusMessage = "Unexpected error while creating room.";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int roomId)
        {
            try
            {
                await _apiClient.DeleteRoomAsync(roomId);
                StatusMessage = "Room deleted successfully.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Cannot delete: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostStatusAsync(int roomId, string newStatus)
        {
            try
            {
                await _apiClient.UpdateRoomStatusAsync(roomId, newStatus);
                StatusMessage = $"Room status updated to '{newStatus}'.";
                StatusMessageType = "success";
            }
            catch (DormApiException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusMessageType = "error";
            }
            return RedirectToPage();
        }
    }
}
