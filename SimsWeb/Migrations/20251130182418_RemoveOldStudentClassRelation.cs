using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimsWeb.Migrations
{
    public partial class RemoveOldStudentClassRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_ClassSections_ClassSectionId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ClassSectionId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ClassSectionId",
                table: "Students");

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ClassSectionId = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_ClassSections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalTable: "ClassSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);  // 👈 Đã sửa

                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);  // 👈 Đã sửa
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassSectionId",
                table: "Enrollments",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.AddColumn<int>(
                name: "ClassSectionId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassSectionId",
                table: "Students",
                column: "ClassSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_ClassSections_ClassSectionId",
                table: "Students",
                column: "ClassSectionId",
                principalTable: "ClassSections",
                principalColumn: "Id");
        }
    }
}
