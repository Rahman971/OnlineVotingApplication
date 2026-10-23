using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVotingApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueVoteConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Votes_StudentId_ElectionId",
                table: "Votes",
                columns: new[] { "StudentId", "ElectionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_StudentId_ElectionId",
                table: "Votes");
        }
    }
}