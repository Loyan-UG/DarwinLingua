using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTalkTopicEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TalkTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TopicGroupKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ArticleBaseText = table.Column<string>(type: "TEXT", maxLength: 12000, nullable: false),
                    EstimatedReadingMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedDiscussionMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SensitivityNote = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    RecommendedForModeratedGroupsOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_TalkTopics", x => x.Id));

            migrationBuilder.CreateTable(
                name: "TalkTopicArticleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 12000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalkTopicArticleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalkTopicArticleTranslations_TalkTopics_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "TalkTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalkTopicQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TalkTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    QuestionType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalkTopicQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalkTopicQuestions_TalkTopics_TalkTopicId",
                        column: x => x.TalkTopicId,
                        principalTable: "TalkTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalkTopicSpeakingGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TalkTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpeakingGoal = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalkTopicSpeakingGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalkTopicSpeakingGoals_TalkTopics_TalkTopicId",
                        column: x => x.TalkTopicId,
                        principalTable: "TalkTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalkTopicTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TalkTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalkTopicTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalkTopicTopics_TalkTopics_TalkTopicId",
                        column: x => x.TalkTopicId,
                        principalTable: "TalkTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TalkTopicTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TalkTopicVocabularyItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TalkTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    WordSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalkTopicVocabularyItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalkTopicVocabularyItems_TalkTopics_TalkTopicId",
                        column: x => x.TalkTopicId,
                        principalTable: "TalkTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalkTopicQuestionTranslations",
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
                    table.PrimaryKey("PK_TalkTopicQuestionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalkTopicQuestionTranslations_TalkTopicQuestions_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "TalkTopicQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_TalkTopicArticleTranslations_OwnerId_LanguageCode", "TalkTopicArticleTranslations", new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicQuestions_TalkTopicId_Kind_SortOrder", "TalkTopicQuestions", new[] { "TalkTopicId", "Kind", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicQuestionTranslations_OwnerId_LanguageCode", "TalkTopicQuestionTranslations", new[] { "OwnerId", "LanguageCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopics_CefrLevel_ContentType_Category", "TalkTopics", new[] { "CefrLevel", "ContentType", "Category" });
            migrationBuilder.CreateIndex("IX_TalkTopics_Slug", "TalkTopics", "Slug", unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicSpeakingGoals_TalkTopicId_SortOrder", "TalkTopicSpeakingGoals", new[] { "TalkTopicId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicSpeakingGoals_TalkTopicId_SpeakingGoal", "TalkTopicSpeakingGoals", new[] { "TalkTopicId", "SpeakingGoal" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicTopics_TalkTopicId", "TalkTopicTopics", "TalkTopicId", unique: true, filter: "\"IsPrimary\"");
            migrationBuilder.CreateIndex("IX_TalkTopicTopics_TalkTopicId_TopicId", "TalkTopicTopics", new[] { "TalkTopicId", "TopicId" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicTopics_TopicId", "TalkTopicTopics", "TopicId");
            migrationBuilder.CreateIndex("IX_TalkTopicVocabularyItems_TalkTopicId_SortOrder", "TalkTopicVocabularyItems", new[] { "TalkTopicId", "SortOrder" }, unique: true);
            migrationBuilder.CreateIndex("IX_TalkTopicVocabularyItems_WordSlug", "TalkTopicVocabularyItems", "WordSlug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TalkTopicArticleTranslations");
            migrationBuilder.DropTable(name: "TalkTopicQuestionTranslations");
            migrationBuilder.DropTable(name: "TalkTopicSpeakingGoals");
            migrationBuilder.DropTable(name: "TalkTopicTopics");
            migrationBuilder.DropTable(name: "TalkTopicVocabularyItems");
            migrationBuilder.DropTable(name: "TalkTopicQuestions");
            migrationBuilder.DropTable(name: "TalkTopics");
        }
    }
}
