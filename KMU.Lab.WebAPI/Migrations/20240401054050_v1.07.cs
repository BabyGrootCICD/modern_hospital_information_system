using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMU.Lab.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class v107 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<char>(
                name: "status",
                table: "tests",
                type: "character(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "tests");
        }
    }
}
