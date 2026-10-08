using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASAHackathon.API.Models
{
    public class Challenge
    {
        [Key]
        public int ChallengeId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Source { get; set; }

        [MaxLength(50)]
        public string? Season { get; set; }

        [Column(TypeName = "text")]
        public string? Metadata { get; set; }

        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
