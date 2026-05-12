using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserContentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserContentProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ContentOwnerType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContentOwnerSlug = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FirstViewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastViewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContentProgress", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserId_ContentOwnerType_ContentOwnerSlug",
                table: "UserContentProgress",
                columns: new[] { "UserId", "ContentOwnerType", "ContentOwnerSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserId_State",
                table: "UserContentProgress",
                columns: new[] { "UserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_UserContentProgress_UserId_UpdatedAtUtc",
                table: "UserContentProgress",
                columns: new[] { "UserId", "UpdatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserContentProgress");
        }
    }
}
