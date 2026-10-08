using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASAHackathon.API.Models
{
    public class GameParticipation
    {
        [Key]
        public int ParticipationId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = null!;

        [Required]
        public int TeamId { get; set; }

        [ForeignKey(nameof(TeamId))]
        public Team Team { get; set; } = null!;

        [Column(TypeName = "text")]
        public string? Answer { get; set; }

        public bool? Correct { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
