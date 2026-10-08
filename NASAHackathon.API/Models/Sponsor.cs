using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASAHackathon.API.Models
{
    public class Sponsor
    {
        [Key]
        public int SponsorId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Logo { get; set; }

        [MaxLength(500)]
        public string? Link { get; set; }

        [Required]
        public int TierId { get; set; }

        [ForeignKey(nameof(TierId))]
        public SponsorTier Tier { get; set; } = null!;

        [Required]
        public bool Active { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
