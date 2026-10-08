using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASAHackathon.API.Models
{
    public class JudgeScore
    {
        [Key]
        public int ScoreId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey(nameof(TeamId))]
        public Team Team { get; set; } = null!;

        [Required]
        public int JudgeId { get; set; }

        [ForeignKey(nameof(JudgeId))]
        public Judge Judge { get; set; } = null!;

        [Required]
        public int CriterionId { get; set; }

        [ForeignKey(nameof(CriterionId))]
        public JudgingCriterion Criterion { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Score { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
