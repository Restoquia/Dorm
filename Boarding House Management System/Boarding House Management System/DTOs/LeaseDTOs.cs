using System.ComponentModel.DataAnnotations;

namespace Boarding_House_Management_System.DTOs
{
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

        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }

    public class LeaseUpdateDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }

    public class LeaseRenewDto
    {
        [Required]
        public DateTime NewEndDate { get; set; }
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
        public string PaymentStatus { get; set; } = "Current"; // "Current", "Overdue", "Fully Paid"
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }
}
