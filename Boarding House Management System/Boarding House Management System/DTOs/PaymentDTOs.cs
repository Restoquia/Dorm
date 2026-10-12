using System.ComponentModel.DataAnnotations;

namespace Boarding_House_Management_System.DTOs
{
    public class PaymentCreateDto
    {
        [Required]
        public int LeaseId { get; set; }

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal AmountPaid { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, GCash, Bank Transfer, Maya, Credit Card

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Completed";
    }

    public class PaymentStatusUpdateDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
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
}
