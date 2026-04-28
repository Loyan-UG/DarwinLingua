using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationStarterPacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversationStarterPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Situation = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Tone = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ConversationGoal = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterPacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStarterLinkedEventPreparationPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPreparationPackSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterLinkedEventPreparationPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationStarterLinkedEventPreparationPacks_ConversationStarterPacks_ConversationStarterPackId",
                        column: x => x.ConversationStarterPackId,
                        principalTable: "ConversationStarterPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStarterLinkedScenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterLinkedScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationStarterLinkedScenarios_ConversationStarterPacks_ConversationStarterPackId",
                        column: x => x.ConversationStarterPackId,
                        principalTable: "ConversationStarterPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStarterPackTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterPackTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationStarterPackTopics_ConversationStarterPacks_ConversationStarterPackId",
                        column: x => x.ConversationStarterPackId,
                        principalTable: "ConversationStarterPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationStarterPackTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStarterPhrases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Function = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    UsageNote = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    Register = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CommonMistake = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterPhrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationStarterPhrases_ConversationStarterPacks_ConversationStarterPackId",
                        column: x => x.ConversationStarterPackId,
                        principalTable: "ConversationStarterPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStarterPhraseAlternatives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPhraseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterPhraseAlternatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationStarterPhraseAlternatives_ConversationStarterPhrases_ConversationStarterPhraseId",
                        column: x => x.ConversationStarterPhraseId,
                        principalTable: "ConversationStarterPhrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStarterPhraseTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPhraseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStarterPhraseTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationStarterPhraseTranslations_ConversationStarterPhrases_ConversationStarterPhraseId",
                        column: x => x.ConversationStarterPhraseId,
                        principalTable: "ConversationStarterPhrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterLinkedEventPreparationPacks_ConversationStarterPackId_EventPreparationPackSlug",
                table: "ConversationStarterLinkedEventPreparationPacks",
                columns: new[] { "ConversationStarterPackId", "EventPreparationPackSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterLinkedScenarios_ConversationStarterPackId_ScenarioSlug",
                table: "ConversationStarterLinkedScenarios",
                columns: new[] { "ConversationStarterPackId", "ScenarioSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPacks_CefrLevel_Situation_Tone_ConversationGoal",
                table: "ConversationStarterPacks",
                columns: new[] { "CefrLevel", "Situation", "Tone", "ConversationGoal" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPacks_Slug",
                table: "ConversationStarterPacks",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPackTopics_ConversationStarterPackId_TopicId",
                table: "ConversationStarterPackTopics",
                columns: new[] { "ConversationStarterPackId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPackTopics_PrimaryPerPack",
                table: "ConversationStarterPackTopics",
                column: "ConversationStarterPackId",
                unique: true,
                filter: "\"IsPrimary\"");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPackTopics_TopicId",
                table: "ConversationStarterPackTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPhraseAlternatives_ConversationStarterPhraseId_SortOrder",
                table: "ConversationStarterPhraseAlternatives",
                columns: new[] { "ConversationStarterPhraseId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPhrases_ConversationStarterPackId_SortOrder",
                table: "ConversationStarterPhrases",
                columns: new[] { "ConversationStarterPackId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStarterPhraseTranslations_ConversationStarterPhraseId_LanguageCode",
                table: "ConversationStarterPhraseTranslations",
                columns: new[] { "ConversationStarterPhraseId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationStarterLinkedEventPreparationPacks");

            migrationBuilder.DropTable(
                name: "ConversationStarterLinkedScenarios");

            migrationBuilder.DropTable(
                name: "ConversationStarterPackTopics");

            migrationBuilder.DropTable(
                name: "ConversationStarterPhraseAlternatives");

            migrationBuilder.DropTable(
                name: "ConversationStarterPhraseTranslations");

            migrationBuilder.DropTable(
                name: "ConversationStarterPhrases");

            migrationBuilder.DropTable(
                name: "ConversationStarterPacks");
        }
    }
}
