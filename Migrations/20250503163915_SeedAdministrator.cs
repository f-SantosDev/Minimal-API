using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimal_API_Project.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdministrator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Administrator",
                columns: new[] { "Id", "Email", "Password", "Profile" },
                values: new object[] { 1, "administrator@teste.com", "123456", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Administrator",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
