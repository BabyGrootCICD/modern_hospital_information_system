using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KMU.HisOrder.MVC.Migrations
{
    public partial class v10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
       
            migrationBuilder.CreateTable(
                name: "testresults",
                columns: table => new
                {
                    testresultid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Billno = table.Column<string>(type: "text", nullable: true),
                    Billdate = table.Column<string>(type: "text", nullable: true),
                    TestGroup = table.Column<string>(type: "text", nullable: true),
                    Reportedby = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    TestName = table.Column<string>(type: "text", nullable: true),
                    Contents = table.Column<string>(type: "text", nullable: true),
                    NvalueMale = table.Column<string>(type: "text", nullable: true),
                    NvalueFemale = table.Column<string>(type: "text", nullable: true),
                    Sufix = table.Column<string>(type: "text", nullable: true),
                    OriOrder = table.Column<string>(type: "text", nullable: true),
                    AttachFile = table.Column<string>(type: "text", nullable: true),
                    refbillorder = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testresults", x => x.testresultid);
                });

            
        }
    }
}
