using System.ComponentModel.DataAnnotations;

namespace OnlineVotingApplication.Models
{
    public class Election
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ElectionName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = false;

        // Results remain private until admin publishes them
        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}