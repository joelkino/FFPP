using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FFPP.Migrations.UserProfiles_Migrations
{
    public partial class InitialCreate_UserProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_userprofiles",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "TEXT", nullable: false),
                    identityProvider = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    userDetails = table.Column<string>(type: "TEXT", nullable: true),
                    theme = table.Column<string>(type: "TEXT", nullable: true),
                    defaultPageSize = table.Column<int>(type: "INTEGER", nullable: true),
                    defaultUseageLocation = table.Column<string>(type: "TEXT", nullable: true),
                    lastTenantName = table.Column<string>(type: "TEXT", nullable: true),
                    lastTenantDomainName = table.Column<string>(type: "TEXT", nullable: true),
                    lastTenantCustomerId = table.Column<string>(type: "TEXT", nullable: true),
                    photoData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__userprofiles", x => x.userId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_userprofiles");
        }
    }
}
