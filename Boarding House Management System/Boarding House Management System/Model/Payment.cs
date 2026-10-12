using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Boarding_House_Management_System.Model
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        [JsonPropertyName("payment_id")]
        public int PaymentId { get; set; }

        [Required]
        [Column("lease_id")]
        [JsonPropertyName("lease_id")]
        public int LeaseId { get; set; }

        [Required]
        [Column("amount_paid", TypeName = "decimal(18,2)")]
        [JsonPropertyName("amount_paid")]
        public decimal AmountPaid { get; set; }

        [Column("payment_date")]
        [JsonPropertyName("payment_date")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        [Column("payment_method")]
        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("reference_number")]
        [JsonPropertyName("reference_number")]
        public string? ReferenceNumber { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey(nameof(LeaseId))]
        [JsonIgnore]
        public Lease? Lease { get; set; }
    }
}
