using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimsWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeleteToClassSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClassSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClassSchedules");
        }
    }
}
