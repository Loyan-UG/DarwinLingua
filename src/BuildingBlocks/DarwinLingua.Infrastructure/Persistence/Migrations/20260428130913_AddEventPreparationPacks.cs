using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventPreparationPacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventPreparationPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPreparationPacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventPreparationLinkedConversationStarterPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPreparationPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationStarterPackSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPreparationLinkedConversationStarterPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPreparationLinkedConversationStarterPacks_EventPreparationPacks_EventPreparationPackId",
                        column: x => x.EventPreparationPackId,
                        principalTable: "EventPreparationPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventPreparationLinkedScenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPreparationPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPreparationLinkedScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPreparationLinkedScenarios_EventPreparationPacks_EventPreparationPackId",
                        column: x => x.EventPreparationPackId,
                        principalTable: "EventPreparationPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventPreparationPackTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPreparationPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPreparationPackTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPreparationPackTopics_EventPreparationPacks_EventPreparationPackId",
                        column: x => x.EventPreparationPackId,
                        principalTable: "EventPreparationPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventPreparationPackTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventPreparationPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPreparationPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PromptType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPreparationPrompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPreparationPrompts_EventPreparationPacks_EventPreparationPackId",
                        column: x => x.EventPreparationPackId,
                        principalTable: "EventPreparationPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventPreparationVocabularyReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPreparationPackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Word = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPreparationVocabularyReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPreparationVocabularyReferences_EventPreparationPacks_EventPreparationPackId",
                        column: x => x.EventPreparationPackId,
                        principalTable: "EventPreparationPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationLinkedConversationStarterPacks_EventPreparationPackId_ConversationStarterPackSlug",
                table: "EventPreparationLinkedConversationStarterPacks",
                columns: new[] { "EventPreparationPackId", "ConversationStarterPackSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationLinkedScenarios_EventPreparationPackId_ScenarioSlug",
                table: "EventPreparationLinkedScenarios",
                columns: new[] { "EventPreparationPackId", "ScenarioSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationPacks_CefrLevel_Category_EventType",
                table: "EventPreparationPacks",
                columns: new[] { "CefrLevel", "Category", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationPacks_Slug",
                table: "EventPreparationPacks",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationPackTopics_EventPreparationPackId_TopicId",
                table: "EventPreparationPackTopics",
                columns: new[] { "EventPreparationPackId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationPackTopics_PrimaryPerPack",
                table: "EventPreparationPackTopics",
                column: "EventPreparationPackId",
                unique: true,
                filter: "\"IsPrimary\"");

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationPackTopics_TopicId",
                table: "EventPreparationPackTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationPrompts_EventPreparationPackId_PromptType_SortOrder",
                table: "EventPreparationPrompts",
                columns: new[] { "EventPreparationPackId", "PromptType", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventPreparationVocabularyReferences_EventPreparationPackId_SortOrder",
                table: "EventPreparationVocabularyReferences",
                columns: new[] { "EventPreparationPackId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventPreparationLinkedConversationStarterPacks");

            migrationBuilder.DropTable(
                name: "EventPreparationLinkedScenarios");

            migrationBuilder.DropTable(
                name: "EventPreparationPackTopics");

            migrationBuilder.DropTable(
                name: "EventPreparationPrompts");

            migrationBuilder.DropTable(
                name: "EventPreparationVocabularyReferences");

            migrationBuilder.DropTable(
                name: "EventPreparationPacks");
        }
    }
}
