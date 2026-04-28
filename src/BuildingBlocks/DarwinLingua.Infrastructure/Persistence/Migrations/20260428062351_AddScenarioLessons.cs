using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScenarioLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScenarioLessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    LearnerGoal = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioDialogueTurns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    SpeakerRole = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    BaseText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioDialogueTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioDialogueTurns_ScenarioLessons_ScenarioLessonId",
                        column: x => x.ScenarioLessonId,
                        principalTable: "ScenarioLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioLessonTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioLessonTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioLessonTopics_ScenarioLessons_ScenarioLessonId",
                        column: x => x.ScenarioLessonId,
                        principalTable: "ScenarioLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioLessonTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioPhrases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    UsageNote = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioPhrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioPhrases_ScenarioLessons_ScenarioLessonId",
                        column: x => x.ScenarioLessonId,
                        principalTable: "ScenarioLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioLessonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioQuestions_ScenarioLessons_ScenarioLessonId",
                        column: x => x.ScenarioLessonId,
                        principalTable: "ScenarioLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioDialogueTurnTranslations",
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
                    table.PrimaryKey("PK_ScenarioDialogueTurnTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioDialogueTurnTranslations_ScenarioDialogueTurns_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "ScenarioDialogueTurns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioPhraseTranslations",
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
                    table.PrimaryKey("PK_ScenarioPhraseTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioPhraseTranslations_ScenarioPhrases_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "ScenarioPhrases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScenarioQuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    Feedback = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioAnswers_ScenarioQuestions_ScenarioQuestionId",
                        column: x => x.ScenarioQuestionId,
                        principalTable: "ScenarioQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioQuestionTranslations",
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
                    table.PrimaryKey("PK_ScenarioQuestionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioQuestionTranslations_ScenarioQuestions_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "ScenarioQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioAnswerTranslations",
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
                    table.PrimaryKey("PK_ScenarioAnswerTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioAnswerTranslations_ScenarioAnswers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "ScenarioAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioAnswers_ScenarioQuestionId_SortOrder",
                table: "ScenarioAnswers",
                columns: new[] { "ScenarioQuestionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioAnswerTranslations_OwnerId_LanguageCode",
                table: "ScenarioAnswerTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioDialogueTurns_ScenarioLessonId_SortOrder",
                table: "ScenarioDialogueTurns",
                columns: new[] { "ScenarioLessonId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioDialogueTurnTranslations_OwnerId_LanguageCode",
                table: "ScenarioDialogueTurnTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioLessons_Slug",
                table: "ScenarioLessons",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioLessonTopics_PrimaryPerLesson",
                table: "ScenarioLessonTopics",
                column: "ScenarioLessonId",
                unique: true,
                filter: "\"IsPrimary\"");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioLessonTopics_ScenarioLessonId_TopicId",
                table: "ScenarioLessonTopics",
                columns: new[] { "ScenarioLessonId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioLessonTopics_TopicId",
                table: "ScenarioLessonTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioPhrases_ScenarioLessonId_SortOrder",
                table: "ScenarioPhrases",
                columns: new[] { "ScenarioLessonId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioPhraseTranslations_OwnerId_LanguageCode",
                table: "ScenarioPhraseTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioQuestions_ScenarioLessonId_SortOrder",
                table: "ScenarioQuestions",
                columns: new[] { "ScenarioLessonId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioQuestionTranslations_OwnerId_LanguageCode",
                table: "ScenarioQuestionTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScenarioAnswerTranslations");

            migrationBuilder.DropTable(
                name: "ScenarioDialogueTurnTranslations");

            migrationBuilder.DropTable(
                name: "ScenarioLessonTopics");

            migrationBuilder.DropTable(
                name: "ScenarioPhraseTranslations");

            migrationBuilder.DropTable(
                name: "ScenarioQuestionTranslations");

            migrationBuilder.DropTable(
                name: "ScenarioAnswers");

            migrationBuilder.DropTable(
                name: "ScenarioDialogueTurns");

            migrationBuilder.DropTable(
                name: "ScenarioPhrases");

            migrationBuilder.DropTable(
                name: "ScenarioQuestions");

            migrationBuilder.DropTable(
                name: "ScenarioLessons");
        }
    }
}
