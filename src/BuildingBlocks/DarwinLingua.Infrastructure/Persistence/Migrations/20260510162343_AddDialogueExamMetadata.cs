using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDialogueExamMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DifficultyNote",
                table: "DialogueLessons",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedPracticeMinutes",
                table: "DialogueLessons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<string>(
                name: "ExamRelevance",
                table: "DialogueLessons",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InteractionMode",
                table: "DialogueLessons",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "face-to-face");

            migrationBuilder.AddColumn<string>(
                name: "Register",
                table: "DialogueLessons",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "neutral");

            migrationBuilder.AddColumn<string>(
                name: "TaskType",
                table: "DialogueLessons",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "exam-roleplay");

            migrationBuilder.CreateTable(
                name: "DialogueExamProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DialogueLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExamProfile = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueExamProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueExamProfiles_DialogueLessons_DialogueLessonId",
                        column: x => x.DialogueLessonId,
                        principalTable: "DialogueLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueSkillFocus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DialogueLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SkillFocus = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueSkillFocus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueSkillFocus_DialogueLessons_DialogueLessonId",
                        column: x => x.DialogueLessonId,
                        principalTable: "DialogueLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueSpeakingFunctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DialogueLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpeakingFunction = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueSpeakingFunctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueSpeakingFunctions_DialogueLessons_DialogueLessonId",
                        column: x => x.DialogueLessonId,
                        principalTable: "DialogueLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueSpeakingPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DialogueLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    PromptType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueSpeakingPrompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueSpeakingPrompts_DialogueLessons_DialogueLessonId",
                        column: x => x.DialogueLessonId,
                        principalTable: "DialogueLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueUsefulWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DialogueLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    WordSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueUsefulWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueUsefulWords_DialogueLessons_DialogueLessonId",
                        column: x => x.DialogueLessonId,
                        principalTable: "DialogueLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialogueSpeakingPromptTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueSpeakingPromptTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueSpeakingPromptTranslations_DialogueSpeakingPrompts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "DialogueSpeakingPrompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_Category",
                table: "DialogueLessons",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_CefrLevel",
                table: "DialogueLessons",
                column: "CefrLevel");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_InteractionMode",
                table: "DialogueLessons",
                column: "InteractionMode");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_Register",
                table: "DialogueLessons",
                column: "Register");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueLessons_TaskType",
                table: "DialogueLessons",
                column: "TaskType");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueExamProfiles_DialogueLessonId_ExamProfile",
                table: "DialogueExamProfiles",
                columns: new[] { "DialogueLessonId", "ExamProfile" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueExamProfiles_ExamProfile",
                table: "DialogueExamProfiles",
                column: "ExamProfile");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueSkillFocus_DialogueLessonId_SkillFocus",
                table: "DialogueSkillFocus",
                columns: new[] { "DialogueLessonId", "SkillFocus" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueSkillFocus_SkillFocus",
                table: "DialogueSkillFocus",
                column: "SkillFocus");

            migrationBuilder.CreateIndex(
                name: "IX_DialogueSpeakingFunctions_DialogueLessonId_SpeakingFunction",
                table: "DialogueSpeakingFunctions",
                columns: new[] { "DialogueLessonId", "SpeakingFunction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueSpeakingPrompts_DialogueLessonId_SortOrder",
                table: "DialogueSpeakingPrompts",
                columns: new[] { "DialogueLessonId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueSpeakingPromptTranslations_OwnerId_LanguageCode",
                table: "DialogueSpeakingPromptTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueUsefulWords_DialogueLessonId_SortOrder",
                table: "DialogueUsefulWords",
                columns: new[] { "DialogueLessonId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueUsefulWords_WordSlug",
                table: "DialogueUsefulWords",
                column: "WordSlug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialogueExamProfiles");

            migrationBuilder.DropTable(
                name: "DialogueSkillFocus");

            migrationBuilder.DropTable(
                name: "DialogueSpeakingFunctions");

            migrationBuilder.DropTable(
                name: "DialogueSpeakingPromptTranslations");

            migrationBuilder.DropTable(
                name: "DialogueUsefulWords");

            migrationBuilder.DropTable(
                name: "DialogueSpeakingPrompts");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_Category",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_CefrLevel",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_InteractionMode",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_Register",
                table: "DialogueLessons");

            migrationBuilder.DropIndex(
                name: "IX_DialogueLessons_TaskType",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "DifficultyNote",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "EstimatedPracticeMinutes",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "ExamRelevance",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "InteractionMode",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "Register",
                table: "DialogueLessons");

            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "DialogueLessons");
        }
    }
}
