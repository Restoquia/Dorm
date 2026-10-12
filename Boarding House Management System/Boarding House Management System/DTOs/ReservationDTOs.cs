using System.ComponentModel.DataAnnotations;

namespace Boarding_House_Management_System.DTOs
{
    public class ReservationCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomId { get; set; }

        public DateTime? ReservationDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Cancelled, Converted
    }

    public class ReservationStatusUpdateDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
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
}
