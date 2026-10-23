using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVotingApplication.Migrations
{
    /// <inheritdoc />
    public partial class SyncCandidateElection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Elections_ElectionId",
                table: "Candidates");

            migrationBuilder.AlterColumn<int>(
                name: "ElectionId",
                table: "Candidates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Elections_ElectionId",
                table: "Candidates",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Elections_ElectionId",
                table: "Candidates");

            migrationBuilder.AlterColumn<int>(
                name: "ElectionId",
                table: "Candidates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Elections_ElectionId",
                table: "Candidates",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
