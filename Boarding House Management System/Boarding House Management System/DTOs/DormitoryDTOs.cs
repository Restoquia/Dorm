using System.ComponentModel.DataAnnotations;

namespace Boarding_House_Management_System.DTOs
{
    public class DormitoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class DormitoryUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(500)]
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
}
