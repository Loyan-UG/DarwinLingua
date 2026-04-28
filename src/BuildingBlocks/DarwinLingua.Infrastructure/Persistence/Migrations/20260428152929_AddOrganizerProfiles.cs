using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizerProfileSlug",
                table: "ConversationEvents",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    OrganizerType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CityRegion = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IsOnlineAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    PublicContactMethod = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    VerificationStatus = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PlanKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    HistoricalEventCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizerProfileHelperLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerProfileHelperLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizerProfileHelperLanguages_OrganizerProfiles_OrganizerProfileId",
                        column: x => x.OrganizerProfileId,
                        principalTable: "OrganizerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizerProfileSupportedLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerProfileSupportedLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizerProfileSupportedLevels_OrganizerProfiles_OrganizerProfileId",
                        column: x => x.OrganizerProfileId,
                        principalTable: "OrganizerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_OrganizerProfileSlug",
                table: "ConversationEvents",
                column: "OrganizerProfileSlug");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfileHelperLanguages_OrganizerProfileId_LanguageCode",
                table: "OrganizerProfileHelperLanguages",
                columns: new[] { "OrganizerProfileId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfiles_CityRegion_PublicationStatus",
                table: "OrganizerProfiles",
                columns: new[] { "CityRegion", "PublicationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfiles_OrganizerType_PublicationStatus",
                table: "OrganizerProfiles",
                columns: new[] { "OrganizerType", "PublicationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfiles_Slug",
                table: "OrganizerProfiles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_CefrLevel",
                table: "OrganizerProfileSupportedLevels",
                columns: new[] { "OrganizerProfileId", "CefrLevel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizerProfileHelperLanguages");

            migrationBuilder.DropTable(
                name: "OrganizerProfileSupportedLevels");

            migrationBuilder.DropTable(
                name: "OrganizerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ConversationEvents_OrganizerProfileSlug",
                table: "ConversationEvents");

            migrationBuilder.DropColumn(
                name: "OrganizerProfileSlug",
                table: "ConversationEvents");
        }
    }
}
