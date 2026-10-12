using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("leases")]
    public class Lease
    {
        [Key]
        [Column("lease_id")]
        [JsonPropertyName("lease_id")]
        public int LeaseId { get; set; }

        [Required]
        [Column("user_id")]
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("room_id")]
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [Required]
        [Column("start_date")]
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

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

        [JsonPropertyName("payments")]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
