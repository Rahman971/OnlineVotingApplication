using Microsoft.EntityFrameworkCore;
using OnlineVotingApplication.Models;

namespace OnlineVotingApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<CandidateApplication> CandidateApplications { get; set; }

        // Password reset tokens
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One student can vote only once in one election
            modelBuilder.Entity<Vote>()
                .HasIndex(v => new
                {
                    v.StudentId,
                    v.ElectionId
                })
                .IsUnique();

            // Candidate belongs to an election
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Election)
                .WithMany()
                .HasForeignKey(c => c.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Password reset token → Admin
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(t => t.Admin)
                .WithMany()
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            // Token should be unique
            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => t.Token)
                .IsUnique();
        }
    }
}