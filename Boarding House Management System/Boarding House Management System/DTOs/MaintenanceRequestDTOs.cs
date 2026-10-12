using System.ComponentModel.DataAnnotations;

namespace Boarding_House_Management_System.DTOs
{
    public class MaintenanceRequestCreateDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string IssueDescription { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Resolved, Cancelled
    }

    public class MaintenanceStatusUpdateDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty; // Pending, In Progress, Resolved, Cancelled
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
}
