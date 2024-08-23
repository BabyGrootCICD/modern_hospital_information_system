using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMU.Lab.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class v105 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "testType",
                table: "tests",
                newName: "testtype");

            migrationBuilder.RenameColumn(
                name: "testName",
                table: "tests",
                newName: "testname");

            migrationBuilder.RenameColumn(
                name: "testCode",
                table: "tests",
                newName: "testcode");

            migrationBuilder.RenameColumn(
                name: "createDate",
                table: "tests",
                newName: "createdate");

            migrationBuilder.RenameColumn(
                name: "patientId",
                table: "patients",
                newName: "patientid");

            migrationBuilder.RenameColumn(
                name: "mobilePhone",
                table: "patients",
                newName: "mobilephone");

            migrationBuilder.RenameColumn(
                name: "birthDate",
                table: "patients",
                newName: "birthdate");

            migrationBuilder.RenameColumn(
                name: "MName",
                table: "patients",
                newName: "mname");

            migrationBuilder.RenameColumn(
                name: "LName",
                table: "patients",
                newName: "lname");

            migrationBuilder.RenameColumn(
                name: "FName",
                table: "patients",
                newName: "fname");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "patients",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "labusers",
                newName: "userid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdate",
                table: "tests",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "testtype",
                table: "tests",
                newName: "testType");

            migrationBuilder.RenameColumn(
                name: "testname",
                table: "tests",
                newName: "testName");

            migrationBuilder.RenameColumn(
                name: "testcode",
                table: "tests",
                newName: "testCode");

            migrationBuilder.RenameColumn(
                name: "createdate",
                table: "tests",
                newName: "createDate");

            migrationBuilder.RenameColumn(
                name: "patientid",
                table: "patients",
                newName: "patientId");

            migrationBuilder.RenameColumn(
                name: "mobilephone",
                table: "patients",
                newName: "mobilePhone");

            migrationBuilder.RenameColumn(
                name: "mname",
                table: "patients",
                newName: "MName");

            migrationBuilder.RenameColumn(
                name: "lname",
                table: "patients",
                newName: "LName");

            migrationBuilder.RenameColumn(
                name: "fname",
                table: "patients",
                newName: "FName");

            migrationBuilder.RenameColumn(
                name: "birthdate",
                table: "patients",
                newName: "birthDate");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "patients",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "labusers",
                newName: "userId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createDate",
                table: "tests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
