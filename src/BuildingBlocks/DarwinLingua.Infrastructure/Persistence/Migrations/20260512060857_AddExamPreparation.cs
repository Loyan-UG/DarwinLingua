using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamPreparation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamPrepUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExamProfileKey = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    ExamSection = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    SkillFocus = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: false),
                    StrategyNotesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ChecklistJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedDialogueSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedTalkTopicSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedGrammarTopicSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExpressionSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedWritingTemplateSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExerciseSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedCourseLessonSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPrepUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CefrRange = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPrepUnits_ExamProfileKey_CefrLevel_ExamSection",
                table: "ExamPrepUnits",
                columns: new[] { "ExamProfileKey", "CefrLevel", "ExamSection" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPrepUnits_Slug",
                table: "ExamPrepUnits",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamProfiles_Key",
                table: "ExamProfiles",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamPrepUnits");

            migrationBuilder.DropTable(
                name: "ExamProfiles");
        }
    }
}
