using System.ComponentModel.DataAnnotations;

namespace NASAHackathon.API.Models
{
    public class Judge
    {
        [Key]
        public int JudgeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Account { get; set; }

        [MaxLength(100)]
        public string? Role { get; set; }

        public string? PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<JudgeScore> JudgeScores { get; set; } = new List<JudgeScore>();
    }
}
