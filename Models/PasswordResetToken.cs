using System.ComponentModel.DataAnnotations;

namespace OnlineVotingApplication.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        [Required]
        public int AdminId { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Admin? Admin { get; set; }
    }
}