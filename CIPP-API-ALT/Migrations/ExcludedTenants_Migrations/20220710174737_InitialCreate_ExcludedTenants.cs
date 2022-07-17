using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIPP_API_ALT.Migrations.ExcludedTenants_Migrations
{
    public partial class InitialCreate_ExcludedTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_excludedTenantEntries",
                columns: table => new
                {
                    TenantDefaultDomain = table.Column<string>(type: "TEXT", nullable: false),
                    DateString = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__excludedTenantEntries", x => x.TenantDefaultDomain);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_excludedTenantEntries");
        }
    }
}
