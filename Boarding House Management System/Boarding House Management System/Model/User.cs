using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("password_hash")]
        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("full_name")]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        [Column("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("phone_number")]
        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("role")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonPropertyName("reservations")]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        [JsonPropertyName("leases")]
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();

        [JsonPropertyName("maintenance_requests")]
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
