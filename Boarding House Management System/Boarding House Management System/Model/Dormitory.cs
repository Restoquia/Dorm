using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("dormitories")]
    public class Dormitory
    {
        [Key]
        [Column("dormitory_id")]
        [JsonPropertyName("dormitory_id")]
        public int DormitoryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("address")]
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [MaxLength(20)]
        [Column("contact_number")]
        [JsonPropertyName("contact_number")]
        public string? ContactNumber { get; set; }

        [MaxLength(500)]
        [Column("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property: A Dormitory has many Rooms
        [JsonPropertyName("rooms")]
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
