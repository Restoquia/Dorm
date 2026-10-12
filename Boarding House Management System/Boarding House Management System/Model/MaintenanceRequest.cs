using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("maintenance_requests")]
    public class MaintenanceRequest
    {
        [Key]
        [Column("request_id")]
        [JsonPropertyName("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("room_id")]
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }

        [Required]
        [Column("user_id")]
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        [Column("issue_description")]
        [JsonPropertyName("issue_description")]
        public string IssueDescription { get; set; } = string.Empty;

        [Column("date_reported")]
        [JsonPropertyName("date_reported")]
        public DateTime DateReported { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        [Column("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [Column("resolved_at")]
        [JsonPropertyName("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        [JsonIgnore]
        public Room? Room { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
