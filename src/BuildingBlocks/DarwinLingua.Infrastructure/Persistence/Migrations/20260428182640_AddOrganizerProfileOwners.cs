using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerProfileOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizerProfileOwners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerProfileSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OwnerEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    AssignedBy = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerProfileOwners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfileOwners_OrganizerProfileSlug_OwnerEmail",
                table: "OrganizerProfileOwners",
                columns: new[] { "OrganizerProfileSlug", "OwnerEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfileOwners_OwnerEmail",
                table: "OrganizerProfileOwners",
                column: "OwnerEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizerProfileOwners");
        }
    }
}
