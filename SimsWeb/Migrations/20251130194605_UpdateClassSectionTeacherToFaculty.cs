using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimsWeb.Migrations
{
    public partial class UpdateClassSectionTeacherToFaculty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Bỏ FK cũ từ ClassSections.TeacherId → AspNetUsers.Id
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSections_AspNetUsers_TeacherId",
                table: "ClassSections");

            // 2. Bỏ index cũ trên TeacherId (string)
            migrationBuilder.DropIndex(
                name: "IX_ClassSections_TeacherId",
                table: "ClassSections");

            // 3. XÓA CỘT TeacherId CŨ (nvarchar, chứa GUID UserId)
            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "ClassSections");

            // 4. TẠO CỘT TeacherId MỚI (int) – FK sang Faculties.Id
            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "ClassSections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 5. Tạo index mới
            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_TeacherId",
                table: "ClassSections",
                column: "TeacherId");

            // 6. Tạo FK mới: ClassSections.TeacherId → Faculties.Id
            migrationBuilder.AddForeignKey(
                name: "FK_ClassSections_Faculties_TeacherId",
                table: "ClassSections",
                column: "TeacherId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Bỏ FK mới sang Faculties
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSections_Faculties_TeacherId",
                table: "ClassSections");

            // 2. Bỏ index mới
            migrationBuilder.DropIndex(
                name: "IX_ClassSections_TeacherId",
                table: "ClassSections");

            // 3. Xóa cột TeacherId (int)
            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "ClassSections");

            // 4. Tạo lại cột TeacherId kiểu string (như cũ, FK sang AspNetUsers)
            migrationBuilder.AddColumn<string>(
                name: "TeacherId",
                table: "ClassSections",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // 5. Tạo lại index
            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_TeacherId",
                table: "ClassSections",
                column: "TeacherId");

            // 6. Tạo lại FK cũ: ClassSections.TeacherId → AspNetUsers.Id
            migrationBuilder.AddForeignKey(
                name: "FK_ClassSections_AspNetUsers_TeacherId",
                table: "ClassSections",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
