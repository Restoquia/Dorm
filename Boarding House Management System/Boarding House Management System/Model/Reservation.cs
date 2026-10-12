using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("reservations")]
    public class Reservation
    {
        [Key]
        [Column("reservation_id")]
        [JsonPropertyName("reservation_id")]
        public int ReservationId { get; set; }

        [Required]
        [Column("user_id")]
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("room_id")]
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [Column("reservation_date")]
        [JsonPropertyName("reservation_date")]
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        [Column("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User? User { get; set; }

        [ForeignKey(nameof(RoomId))]
        [JsonIgnore]
        public Room? Room { get; set; }
    }
}
