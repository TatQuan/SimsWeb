using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimsWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExerciseFilePath",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuideFilePath",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseFilePath",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "GuideFilePath",
                table: "Assignments");
        }
    }
}
