using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIPP_API_ALT.Migrations.CippLogs_Migrations
{
    public partial class InitialCreate_CippLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_logEntries",
                columns: table => new
                {
                    RowKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    API = table.Column<string>(type: "TEXT", nullable: true),
                    SentAsAlert = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tenant = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__logEntries", x => x.RowKey);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_logEntries");
        }
    }
}
