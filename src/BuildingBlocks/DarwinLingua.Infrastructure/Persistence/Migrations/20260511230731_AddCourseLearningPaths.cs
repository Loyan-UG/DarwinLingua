using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseLearningPaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoursePaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    CefrRange = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CoursePathId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CoursePathSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ModuleNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseModules_CoursePaths_CoursePathId",
                        column: x => x.CoursePathId,
                        principalTable: "CoursePaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseLessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CoursePathSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ModuleSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LessonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Narrative = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    LearningGoalsJson = table.Column<string>(type: "TEXT", nullable: false),
                    PrerequisiteLessonSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    NextLessonSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    LinkedGrammarTopicSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedWordSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExpressionSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedDialogueSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedTalkTopicSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExerciseSetSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExamPrepSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    ReviewSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    HomeworkTask = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseLessons_CourseModules_CourseModuleId",
                        column: x => x.CourseModuleId,
                        principalTable: "CourseModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_CourseModuleId_LessonNumber",
                table: "CourseLessons",
                columns: new[] { "CourseModuleId", "LessonNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_CoursePathSlug_ModuleSlug",
                table: "CourseLessons",
                columns: new[] { "CoursePathSlug", "ModuleSlug" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_Slug",
                table: "CourseLessons",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_CoursePathId_ModuleNumber",
                table: "CourseModules",
                columns: new[] { "CoursePathId", "ModuleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_Slug",
                table: "CourseModules",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePaths_CefrLevel",
                table: "CoursePaths",
                column: "CefrLevel");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePaths_Slug",
                table: "CoursePaths",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseLessons");

            migrationBuilder.DropTable(
                name: "CourseModules");

            migrationBuilder.DropTable(
                name: "CoursePaths");
        }
    }
}
