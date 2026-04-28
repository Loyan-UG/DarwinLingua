using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerClaimRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizerClaimRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerProfileSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RequesterName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RequesterEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    RelationshipToOrganizer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EvidenceText = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerClaimRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerClaimRequests_OrganizerProfileSlug_Status",
                table: "OrganizerClaimRequests",
                columns: new[] { "OrganizerProfileSlug", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerClaimRequests_RequesterEmail_OrganizerProfileSlug_Status",
                table: "OrganizerClaimRequests",
                columns: new[] { "RequesterEmail", "OrganizerProfileSlug", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizerClaimRequests");
        }
    }
}
