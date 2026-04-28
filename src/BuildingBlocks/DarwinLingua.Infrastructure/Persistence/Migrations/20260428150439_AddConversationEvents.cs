using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CountryRegion = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ApproximateLocation = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    IsOnline = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OrganizerName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ExternalLink = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    ContactMethod = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ScheduleText = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PriceType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    VerificationStatus = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    LastVerifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationEventHelperLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationEventHelperLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationEventHelperLanguages_ConversationEvents_ConversationEventId",
                        column: x => x.ConversationEventId,
                        principalTable: "ConversationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationEventLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationEventLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationEventLevels_ConversationEvents_ConversationEventId",
                        column: x => x.ConversationEventId,
                        principalTable: "ConversationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationEventPreparationPackLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreparationPackSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationEventPreparationPackLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationEventPreparationPackLinks_ConversationEvents_ConversationEventId",
                        column: x => x.ConversationEventId,
                        principalTable: "ConversationEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEventHelperLanguages_ConversationEventId_LanguageCode",
                table: "ConversationEventHelperLanguages",
                columns: new[] { "ConversationEventId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEventLevels_ConversationEventId_CefrLevel",
                table: "ConversationEventLevels",
                columns: new[] { "ConversationEventId", "CefrLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEventPreparationPackLinks_ConversationEventId_PreparationPackSlug",
                table: "ConversationEventPreparationPackLinks",
                columns: new[] { "ConversationEventId", "PreparationPackSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Category_PublicationStatus",
                table: "ConversationEvents",
                columns: new[] { "Category", "PublicationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_City_IsOnline_PriceType",
                table: "ConversationEvents",
                columns: new[] { "City", "IsOnline", "PriceType" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Slug",
                table: "ConversationEvents",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationEventHelperLanguages");

            migrationBuilder.DropTable(
                name: "ConversationEventLevels");

            migrationBuilder.DropTable(
                name: "ConversationEventPreparationPackLinks");

            migrationBuilder.DropTable(
                name: "ConversationEvents");
        }
    }
}
