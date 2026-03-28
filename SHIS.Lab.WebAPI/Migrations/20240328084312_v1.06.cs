using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHIS.Lab.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class v106 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "inhospid",
                table: "tests",
                newName: "labrequestid");

            migrationBuilder.RenameColumn(
                name: "testId",
                table: "tests",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "labrequestid",
                table: "tests",
                newName: "inhospid");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "tests",
                newName: "testId");
        }
    }
}
