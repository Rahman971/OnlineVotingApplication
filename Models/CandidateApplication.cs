using System.ComponentModel.DataAnnotations;

namespace OnlineVotingApplication.Models
{
    public class CandidateApplication
    {
        public int Id { get; set; }

        // Student Details
        [Required]
        [StringLength(20)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Year { get; set; } = string.Empty;


        // Election Details
        [Required]
        public int ElectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;


        // Candidate Information
        [StringLength(500)]
        public string? CandidatePhoto { get; set; }

        [StringLength(2000)]
        public string? CandidateDetails { get; set; }


        // Application Status
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }
    }
}