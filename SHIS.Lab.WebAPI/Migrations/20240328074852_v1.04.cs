using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHIS.Lab.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class v104 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "plan_des",
                table: "tests",
                newName: "testType");

            migrationBuilder.RenameColumn(
                name: "plan_code",
                table: "tests",
                newName: "testName");

            migrationBuilder.RenameColumn(
                name: "hplan_type",
                table: "tests",
                newName: "testCode");

            migrationBuilder.AddColumn<DateTime>(
                name: "createDate",
                table: "tests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createDate",
                table: "tests");

            migrationBuilder.RenameColumn(
                name: "testType",
                table: "tests",
                newName: "plan_des");

            migrationBuilder.RenameColumn(
                name: "testName",
                table: "tests",
                newName: "plan_code");

            migrationBuilder.RenameColumn(
                name: "testCode",
                table: "tests",
                newName: "hplan_type");
        }
    }
}
