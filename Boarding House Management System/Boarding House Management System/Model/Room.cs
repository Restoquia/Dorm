using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("room_id")]
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [Column("dormitory_id")]
        [JsonPropertyName("dormitory_id")]
        public int? DormitoryId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("room_number")]
        [JsonPropertyName("room_number")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("room_type")]
        [JsonPropertyName("room_type")]
        public string RoomType { get; set; } = string.Empty;

        [Column("capacity")]
        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [Column("current_occupants")]
        [JsonPropertyName("current_occupants")]
        public int CurrentOccupants { get; set; } = 0;

        [Column("monthly_rate", TypeName = "decimal(18,2)")]
        [JsonPropertyName("monthly_rate")]
        public decimal MonthlyRate { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey(nameof(DormitoryId))]
        [JsonIgnore]
        public Dormitory? Dormitory { get; set; }

        [JsonPropertyName("reservations")]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        [JsonPropertyName("leases")]
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();

        [JsonPropertyName("maintenance_requests")]
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
