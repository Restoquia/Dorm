using System.Net.Http.Json;
using System.Text.Json;
using Dorm.Models;

namespace Dorm.Services
{
    public class DormApiException : Exception
    {
        public int StatusCode { get; }
        public DormApiException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class DormApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<DormApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DormApiClient(HttpClient http, IConfiguration config, ILogger<DormApiClient> logger)
        {
            _http = http;
            _logger = logger;

            var baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5210/api/";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _http.BaseAddress = new Uri(baseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private async Task HandleErrorAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string errorMessage = $"API Error: {(int)response.StatusCode} {response.ReasonPhrase}";
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(content, _jsonOptions);
                        if (!string.IsNullOrWhiteSpace(errorObj?.Message))
                        {
                            errorMessage = errorObj.Message;
                        }
                    }
                    catch
                    {
                        errorMessage = content;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse API error response body.");
            }

            throw new DormApiException(errorMessage, (int)response.StatusCode);
        }

        // ==========================================
        // 1. AUTHENTICATION & USERS
        // ==========================================
        public async Task<UserResponseDto?> LoginAsync(UserLoginDto dto)
        {
            var res = await _http.PostAsJsonAsync("users/login", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<UserResponseDto>(_jsonOptions);
        }

        public async Task<UserResponseDto?> RegisterAsync(UserCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("users", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<UserResponseDto>(_jsonOptions);
        }

        public async Task<List<UserResponseDto>> GetUsersAsync(string? role = null, string? search = null)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(role)) query.Add($"role={Uri.EscapeDataString(role)}");
            if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");

            var url = "users" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<UserResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<UserDetailDto>($"users/{id}", _jsonOptions);
        }

        // ==========================================
        // 2. DORMITORIES
        // ==========================================
        public async Task<List<DormitoryResponseDto>> GetDormitoriesAsync()
        {
            return await _http.GetFromJsonAsync<List<DormitoryResponseDto>>("dormitories", _jsonOptions) ?? new();
        }

        public async Task<DormitoryDetailDto?> GetDormitoryByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<DormitoryDetailDto>($"dormitories/{id}", _jsonOptions);
        }

        public async Task<DormitoryResponseDto?> CreateDormitoryAsync(DormitoryCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("dormitories", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<DormitoryResponseDto>(_jsonOptions);
        }

        // ==========================================
        // 3. ROOMS
        // ==========================================
        public async Task<List<RoomResponseDto>> GetRoomsAsync(int? dormId = null, string? status = null, string? roomType = null, decimal? minRate = null, decimal? maxRate = null, string? search = null)
        {
            var query = new List<string>();
            if (dormId.HasValue) query.Add($"dormitoryId={dormId.Value}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(roomType)) query.Add($"roomType={Uri.EscapeDataString(roomType)}");
            if (minRate.HasValue) query.Add($"minRate={minRate.Value}");
            if (maxRate.HasValue) query.Add($"maxRate={maxRate.Value}");
            if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");

            var url = "rooms" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<RoomResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<List<RoomResponseDto>> GetAvailableRoomsAsync(int? dormId = null, string? roomType = null, decimal? maxRate = null)
        {
            var query = new List<string>();
            if (dormId.HasValue) query.Add($"dormitoryId={dormId.Value}");
            if (!string.IsNullOrEmpty(roomType)) query.Add($"roomType={Uri.EscapeDataString(roomType)}");
            if (maxRate.HasValue) query.Add($"maxRate={maxRate.Value}");

            var url = "rooms/available" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<RoomResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<RoomResponseDto?> GetRoomByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<RoomResponseDto>($"rooms/{id}", _jsonOptions);
        }

        public async Task<RoomResponseDto?> CreateRoomAsync(RoomCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("rooms", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<RoomResponseDto>(_jsonOptions);
        }

        public async Task<RoomResponseDto?> UpdateRoomAsync(int id, RoomUpdateDto dto)
        {
            var res = await _http.PutAsJsonAsync($"rooms/{id}", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<RoomResponseDto>(_jsonOptions);
        }

        public async Task UpdateRoomStatusAsync(int id, string status)
        {
            var res = await _http.PatchAsJsonAsync($"rooms/{id}/status", new RoomStatusUpdateDto { Status = status });
            await HandleErrorAsync(res);
        }

        public async Task DeleteRoomAsync(int id)
        {
            var res = await _http.DeleteAsync($"rooms/{id}");
            await HandleErrorAsync(res);
        }

        // ==========================================
        // 4. RESERVATIONS
        // ==========================================
        public async Task<List<ReservationResponseDto>> GetReservationsAsync(int? userId = null, int? roomId = null, string? status = null)
        {
            var query = new List<string>();
            if (userId.HasValue) query.Add($"userId={userId.Value}");
            if (roomId.HasValue) query.Add($"roomId={roomId.Value}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

            var url = "reservations" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<ReservationResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<ReservationResponseDto?> CreateReservationAsync(ReservationCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("reservations", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<ReservationResponseDto>(_jsonOptions);
        }

        public async Task ApproveReservationAsync(int id)
        {
            var res = await _http.PutAsync($"reservations/{id}/approve", null);
            await HandleErrorAsync(res);
        }

        public async Task CancelReservationAsync(int id)
        {
            var res = await _http.PutAsync($"reservations/{id}/cancel", null);
            await HandleErrorAsync(res);
        }

        public async Task<LeaseResponseDto?> ConvertReservationToLeaseAsync(int id, ReservationConvertToLeaseDto dto)
        {
            var res = await _http.PostAsJsonAsync($"reservations/{id}/convert-to-lease", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<LeaseResponseDto>(_jsonOptions);
        }

        // ==========================================
        // 5. LEASES
        // ==========================================
        public async Task<List<LeaseResponseDto>> GetLeasesAsync(int? userId = null, int? roomId = null, string? status = null)
        {
            var query = new List<string>();
            if (userId.HasValue) query.Add($"userId={userId.Value}");
            if (roomId.HasValue) query.Add($"roomId={roomId.Value}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

            var url = "leases" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<LeaseResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<List<LeaseResponseDto>> GetActiveLeasesAsync()
        {
            return await _http.GetFromJsonAsync<List<LeaseResponseDto>>("leases/active", _jsonOptions) ?? new();
        }

        public async Task<LeaseResponseDto?> GetLeaseByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<LeaseResponseDto>($"leases/{id}", _jsonOptions);
        }

        public async Task<LeaseResponseDto?> CreateLeaseAsync(LeaseCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("leases", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<LeaseResponseDto>(_jsonOptions);
        }

        public async Task TerminateLeaseAsync(int id)
        {
            var res = await _http.PostAsync($"leases/{id}/terminate", null);
            await HandleErrorAsync(res);
        }

        // ==========================================
        // 6. PAYMENTS & FINANCIALS
        // ==========================================
        public async Task<PaymentResponseDto?> PayRentAsync(PaymentCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("payments", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<PaymentResponseDto>(_jsonOptions);
        }

        public async Task<List<PaymentResponseDto>> GetPaymentsAsync(int? leaseId = null, int? userId = null, string? paymentMethod = null, string? status = null)
        {
            var query = new List<string>();
            if (leaseId.HasValue) query.Add($"leaseId={leaseId.Value}");
            if (userId.HasValue) query.Add($"userId={userId.Value}");
            if (!string.IsNullOrEmpty(paymentMethod)) query.Add($"paymentMethod={Uri.EscapeDataString(paymentMethod)}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

            var url = "payments" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<PaymentResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<LeaseBalanceDto?> GetLeaseStatementOfAccountAsync(int leaseId)
        {
            return await _http.GetFromJsonAsync<LeaseBalanceDto>($"payments/lease/{leaseId}", _jsonOptions);
        }

        public async Task<PaymentSummaryDto?> GetPaymentSummaryAsync()
        {
            return await _http.GetFromJsonAsync<PaymentSummaryDto>("payments/summary", _jsonOptions);
        }

        // ==========================================
        // 7. MAINTENANCE REQUESTS
        // ==========================================
        public async Task<List<MaintenanceRequestResponseDto>> GetMaintenanceRequestsAsync(int? roomId = null, int? userId = null, string? status = null)
        {
            var query = new List<string>();
            if (roomId.HasValue) query.Add($"roomId={roomId.Value}");
            if (userId.HasValue) query.Add($"userId={userId.Value}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

            var url = "maintenancerequests" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await _http.GetFromJsonAsync<List<MaintenanceRequestResponseDto>>(url, _jsonOptions) ?? new();
        }

        public async Task<MaintenanceRequestResponseDto?> CreateMaintenanceRequestAsync(MaintenanceRequestCreateDto dto)
        {
            var res = await _http.PostAsJsonAsync("maintenancerequests", dto);
            await HandleErrorAsync(res);
            return await res.Content.ReadFromJsonAsync<MaintenanceRequestResponseDto>(_jsonOptions);
        }

        public async Task UpdateMaintenanceStatusAsync(int id, string status)
        {
            var res = await _http.PutAsJsonAsync($"maintenancerequests/{id}/status", new MaintenanceStatusUpdateDto { Status = status });
            await HandleErrorAsync(res);
        }

        // ==========================================
        // 8. DASHBOARD & ANALYTICS
        // ==========================================
        public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
        {
            return await _http.GetFromJsonAsync<DashboardStatsDto>("dashboard/stats", _jsonOptions);
        }

        // ==========================================
        // 9. DATABASE SEED UTILITY
        // ==========================================
        public async Task ResetSeedDataAsync()
        {
            var res = await _http.PostAsync("seed/reset", null);
            await HandleErrorAsync(res);
        }
    }
}
