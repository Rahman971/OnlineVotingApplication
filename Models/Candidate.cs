using System.ComponentModel.DataAnnotations;

namespace OnlineVotingApplication.Models
{
    public class Candidate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CandidateId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Manifesto { get; set; }

        public string? PhotoPath { get; set; }

        public bool IsApproved { get; set; } = false;

        public int ElectionId { get; set; }

        public Election? Election { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}