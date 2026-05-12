using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Instruction = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    ExerciseType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TargetSkill = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OwnerSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PromptJson = table.Column<string>(type: "TEXT", nullable: false),
                    AnswerKeyJson = table.Column<string>(type: "TEXT", nullable: false),
                    CorrectExplanation = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IncorrectExplanation = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Hint = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CommonMistakeNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OwnerSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserExerciseAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ExerciseSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SubmittedAnswerJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    FeedbackExplanation = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExerciseAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExerciseSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExerciseSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSetItems_ExerciseSets_ExerciseSetId",
                        column: x => x.ExerciseSetId,
                        principalTable: "ExerciseSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CefrLevel_ExerciseType_TargetSkill",
                table: "Exercises",
                columns: new[] { "CefrLevel", "ExerciseType", "TargetSkill" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_OwnerType_OwnerSlug",
                table: "Exercises",
                columns: new[] { "OwnerType", "OwnerSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Slug",
                table: "Exercises",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSetItems_ExerciseSetId_ExerciseSlug",
                table: "ExerciseSetItems",
                columns: new[] { "ExerciseSetId", "ExerciseSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSetItems_ExerciseSetId_SortOrder",
                table: "ExerciseSetItems",
                columns: new[] { "ExerciseSetId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_OwnerType_OwnerSlug",
                table: "ExerciseSets",
                columns: new[] { "OwnerType", "OwnerSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_Slug",
                table: "ExerciseSets",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseAttempts_UserId_ExerciseSlug_AttemptedAtUtc",
                table: "UserExerciseAttempts",
                columns: new[] { "UserId", "ExerciseSlug", "AttemptedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "ExerciseSetItems");

            migrationBuilder.DropTable(
                name: "UserExerciseAttempts");

            migrationBuilder.DropTable(
                name: "ExerciseSets");
        }
    }
}
