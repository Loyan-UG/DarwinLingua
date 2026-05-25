using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleplayScenarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleplayScenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LinkedDialogueSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    LearnerGoal = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    InteractionMode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Register = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    EstimatedPracticeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    ExamProfilesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SkillFocusJson = table.Column<string>(type: "TEXT", nullable: false),
                    RolesJson = table.Column<string>(type: "TEXT", nullable: false),
                    TurnsJson = table.Column<string>(type: "TEXT", nullable: false),
                    AnswerChoicesJson = table.Column<string>(type: "TEXT", nullable: false),
                    StaticFeedbackJson = table.Column<string>(type: "TEXT", nullable: false),
                    ImageSlotsJson = table.Column<string>(type: "TEXT", nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplayScenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleplayScenarioTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleplayScenarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplayScenarioTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleplayScenarioTopics_RoleplayScenarios_RoleplayScenarioId",
                        column: x => x.RoleplayScenarioId,
                        principalTable: "RoleplayScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleplayScenarioTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_CefrLevel_Category_TaskType_InteractionMode_Register",
                table: "RoleplayScenarios",
                columns: new[] { "CefrLevel", "Category", "TaskType", "InteractionMode", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarios_Slug",
                table: "RoleplayScenarios",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarioTopics_RoleplayScenarioId_TopicId",
                table: "RoleplayScenarioTopics",
                columns: new[] { "RoleplayScenarioId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayScenarioTopics_TopicId",
                table: "RoleplayScenarioTopics",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleplayScenarioTopics");

            migrationBuilder.DropTable(
                name: "RoleplayScenarios");
        }
    }
}
