using System.ComponentModel.DataAnnotations;

namespace Boarding_House_Management_System.DTOs
{
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

        [MaxLength(20)]
        public string Status { get; set; } = "Available";
    }

    public class RoomUpdateDto
    {
        public int? DormitoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RoomType { get; set; } = string.Empty;

        [Range(1, 50)]
        public int Capacity { get; set; }

        [Range(0, 50)]
        public int CurrentOccupants { get; set; }

        [Range(0, 1000000)]
        public decimal MonthlyRate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Available";
    }

    public class RoomStatusUpdateDto
    {
        [Required]
        [MaxLength(20)]
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
}
