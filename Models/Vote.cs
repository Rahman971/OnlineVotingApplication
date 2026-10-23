using System.ComponentModel.DataAnnotations;

namespace OnlineVotingApplication.Models
{
    public class Vote
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int ElectionId { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.Now;

        // Relationships
        public Student? Student { get; set; }

        public Candidate? Candidate { get; set; }

        public Election? Election { get; set; }
    }
}