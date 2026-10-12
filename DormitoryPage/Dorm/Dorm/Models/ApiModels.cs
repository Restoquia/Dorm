using System.ComponentModel.DataAnnotations;

namespace Dorm.Models
{
    // ==========================================
    // USER DTOs
    // ==========================================
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(4)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = "Tenant";
    }

    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ActiveLeasesCount { get; set; }
    }

    public class UserDetailDto : UserResponseDto
    {
        public List<LeaseResponseDto> Leases { get; set; } = new();
        public List<ReservationResponseDto> Reservations { get; set; } = new();
        public List<MaintenanceRequestResponseDto> MaintenanceRequests { get; set; } = new();
    }

    // ==========================================
    // DORMITORY DTOs
    // ==========================================
    public class DormitoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
    }

    public class DormitoryResponseDto
    {
        public int DormitoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentOccupants { get; set; }
        public double OccupancyRate => TotalCapacity > 0 ? Math.Round((double)CurrentOccupants / TotalCapacity * 100, 1) : 0;
    }

    public class DormitoryDetailDto : DormitoryResponseDto
    {
        public List<RoomResponseDto> Rooms { get; set; } = new();
    }

    // ==========================================
    // ROOM DTOs
    // ==========================================
    public class RoomCreateDto
    {
        public int? DormitoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RoomType { get; set; } = string.Empty;

        [Range(1, 50)]
        public int Capacity { get; set; } = 1;

        [Range(0, 50)]
        public int CurrentOccupants { get; set; } = 0;

        [Range(0, 1000000)]
        public decimal MonthlyRate { get; set; }

        public string Status { get; set; } = "Available";
    }

    public class RoomUpdateDto
    {
        public int? DormitoryId { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public string RoomType { get; set; } = string.Empty;

        public int Capacity { get; set; }
        public int CurrentOccupants { get; set; }
        public decimal MonthlyRate { get; set; }
        public string Status { get; set; } = "Available";
    }

    public class RoomStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class RoomResponseDto
    {
        public int RoomId { get; set; }
        public int? DormitoryId { get; set; }
        public string? DormitoryName { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentOccupants { get; set; }
        public int AvailableSlots => Math.Max(0, Capacity - CurrentOccupants);
        public decimal MonthlyRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAvailable => Status.Equals("Available", StringComparison.OrdinalIgnoreCase) && CurrentOccupants < Capacity;
    }

    // ==========================================
    // RESERVATION DTOs
    // ==========================================
    public class ReservationCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomId { get; set; }

        public DateTime? ReservationDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";
    }

    public class ReservationConvertToLeaseDto
    {
        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(6);
    }

    public class ReservationResponseDto
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? DormitoryName { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ==========================================
    // LEASE DTOs
    // ==========================================
    public class LeaseCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(6);

        public string Status { get; set; } = "Active";
    }

    public class LeaseResponseDto
    {
        public int LeaseId { get; set; }
        public int UserId { get; set; }
        public string? TenantName { get; set; }
        public string? TenantEmail { get; set; }
        public string? TenantPhone { get; set; }
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public decimal MonthlyRate { get; set; }
        public string? DormitoryName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmountPaid { get; set; }
        public decimal EstimatedTotalRent { get; set; }
        public decimal BalanceRemaining { get; set; }
        public int PaymentCount { get; set; }
    }

    public class LeaseBalanceDto
    {
        public int LeaseId { get; set; }
        public string? TenantName { get; set; }
        public string? RoomNumber { get; set; }
        public decimal MonthlyRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalMonths { get; set; }
        public decimal ExpectedTotalRent { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentStatus { get; set; } = "Current";
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }

    // ==========================================
    // PAYMENT DTOs
    // ==========================================
    public class PaymentCreateDto
    {
        [Required]
        public int LeaseId { get; set; }

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal AmountPaid { get; set; }

        public DateTime? PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string PaymentMethod { get; set; } = "GCash";

        public string? ReferenceNumber { get; set; }

        public string Status { get; set; } = "Completed";
    }

    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int LeaseId { get; set; }
        public string? TenantName { get; set; }
        public int? UserId { get; set; }
        public string? RoomNumber { get; set; }
        public string? DormitoryName { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal LeaseMonthlyRate { get; set; }
    }

    public class PaymentSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<string, decimal> RevenueByMethod { get; set; } = new();
        public List<PaymentResponseDto> RecentPayments { get; set; } = new();
    }

    // ==========================================
    // MAINTENANCE DTOs
    // ==========================================
    public class MaintenanceRequestCreateDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string IssueDescription { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
    }

    public class MaintenanceStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class MaintenanceRequestResponseDto
    {
        public int RequestId { get; set; }
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? DormitoryName { get; set; }
        public int UserId { get; set; }
        public string? RequesterName { get; set; }
        public string? RequesterContact { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
    }

    // ==========================================
    // DASHBOARD STATS DTO
    // ==========================================
    public class DashboardStatsDto
    {
        public int TotalDormitories { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int UnderMaintenanceRooms { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalOccupants { get; set; }
        public double OverallOccupancyRate { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveLeases { get; set; }
        public int PendingReservations { get; set; }
        public int PendingMaintenanceRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public List<RoomResponseDto> AvailableRoomsList { get; set; } = new();
        public List<PaymentResponseDto> RecentPayments { get; set; } = new();
        public List<MaintenanceRequestResponseDto> RecentMaintenanceRequests { get; set; } = new();
    }

    // API Standard Error representation
    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
