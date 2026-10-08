using System.ComponentModel.DataAnnotations;

namespace NASAHackathon.API.Models
{
    public class SponsorTier
    {
        [Key]
        public int TierId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? Priority { get; set; }

        [Required]
        public bool BannerEnabled { get; set; }

        public ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
    }
}
