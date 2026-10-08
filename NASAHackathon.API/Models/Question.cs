using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASAHackathon.API.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int ChallengeId { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Text { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Options { get; set; }

        [Column(TypeName = "text")]
        public string? CorrectAnswer { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<GameParticipation> GameParticipations { get; set; } = new List<GameParticipation>();
    }
}
