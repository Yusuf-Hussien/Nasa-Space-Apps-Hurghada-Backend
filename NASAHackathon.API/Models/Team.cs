using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASAHackathon.API.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int ChallengeId { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "active";

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public Project? Project { get; set; }
        public ICollection<JudgeScore> JudgeScores { get; set; } = new List<JudgeScore>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        public ICollection<GameParticipation> GameParticipations { get; set; } = new List<GameParticipation>();
    }
}
