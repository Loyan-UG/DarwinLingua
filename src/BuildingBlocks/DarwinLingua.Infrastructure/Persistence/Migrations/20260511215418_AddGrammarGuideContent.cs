using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGrammarGuideContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrammarTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    GrammarCategory = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrammarCommonMistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    WrongText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CorrectedText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarCommonMistakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarCommonMistakes_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarExamples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    GermanText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarExamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarExamples_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarExceptionNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarExceptionNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarExceptionNotes_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarLinkedDialogues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarLinkedDialogues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarLinkedDialogues_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarLinkedExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarLinkedExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarLinkedExercises_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarLinkedTalkTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarLinkedTalkTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarLinkedTalkTopics_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarLinkedWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    WordSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarLinkedWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarLinkedWords_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarPrerequisiteLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarPrerequisiteLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarPrerequisiteLinks_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarRelatedTopicLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarRelatedTopicLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarRelatedTopicLinks_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarRuleSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarRuleSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarRuleSummaries_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 12000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarSections_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarTopicTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrammarTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarTopicTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarTopicTopics_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrammarTopicTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrammarCommonMistakeTranslations",
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
                    table.PrimaryKey("PK_GrammarCommonMistakeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarCommonMistakeTranslations_GrammarCommonMistakes_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GrammarCommonMistakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarExampleTranslations",
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
                    table.PrimaryKey("PK_GrammarExampleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarExampleTranslations_GrammarExamples_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GrammarExamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarExceptionNoteTranslations",
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
                    table.PrimaryKey("PK_GrammarExceptionNoteTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarExceptionNoteTranslations_GrammarExceptionNotes_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GrammarExceptionNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarRuleSummaryTranslations",
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
                    table.PrimaryKey("PK_GrammarRuleSummaryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarRuleSummaryTranslations_GrammarRuleSummaries_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GrammarRuleSummaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarSectionTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 12000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarSectionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarSectionTranslations_GrammarSections_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GrammarSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrammarCommonMistakes_GrammarTopicId_SortOrder",
                table: "GrammarCommonMistakes",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarCommonMistakeTranslations_OwnerId_LanguageCode",
                table: "GrammarCommonMistakeTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarExamples_GrammarTopicId_SortOrder",
                table: "GrammarExamples",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarExampleTranslations_OwnerId_LanguageCode",
                table: "GrammarExampleTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarExceptionNotes_GrammarTopicId_SortOrder",
                table: "GrammarExceptionNotes",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarExceptionNoteTranslations_OwnerId_LanguageCode",
                table: "GrammarExceptionNoteTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedDialogues_GrammarTopicId_SortOrder",
                table: "GrammarLinkedDialogues",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedDialogues_GrammarTopicId_TargetSlug",
                table: "GrammarLinkedDialogues",
                columns: new[] { "GrammarTopicId", "TargetSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedExercises_GrammarTopicId_SortOrder",
                table: "GrammarLinkedExercises",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedExercises_GrammarTopicId_TargetSlug",
                table: "GrammarLinkedExercises",
                columns: new[] { "GrammarTopicId", "TargetSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedTalkTopics_GrammarTopicId_SortOrder",
                table: "GrammarLinkedTalkTopics",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedTalkTopics_GrammarTopicId_TargetSlug",
                table: "GrammarLinkedTalkTopics",
                columns: new[] { "GrammarTopicId", "TargetSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedWords_GrammarTopicId_SortOrder",
                table: "GrammarLinkedWords",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarLinkedWords_WordSlug",
                table: "GrammarLinkedWords",
                column: "WordSlug");

            migrationBuilder.CreateIndex(
                name: "IX_GrammarPrerequisiteLinks_GrammarTopicId_SortOrder",
                table: "GrammarPrerequisiteLinks",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarPrerequisiteLinks_GrammarTopicId_TargetSlug",
                table: "GrammarPrerequisiteLinks",
                columns: new[] { "GrammarTopicId", "TargetSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarRelatedTopicLinks_GrammarTopicId_SortOrder",
                table: "GrammarRelatedTopicLinks",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarRelatedTopicLinks_GrammarTopicId_TargetSlug",
                table: "GrammarRelatedTopicLinks",
                columns: new[] { "GrammarTopicId", "TargetSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarRuleSummaries_GrammarTopicId_SortOrder",
                table: "GrammarRuleSummaries",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarRuleSummaryTranslations_OwnerId_LanguageCode",
                table: "GrammarRuleSummaryTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarSections_GrammarTopicId_SortOrder",
                table: "GrammarSections",
                columns: new[] { "GrammarTopicId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarSectionTranslations_OwnerId_LanguageCode",
                table: "GrammarSectionTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopics_CefrLevel_GrammarCategory",
                table: "GrammarTopics",
                columns: new[] { "CefrLevel", "GrammarCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopics_Slug",
                table: "GrammarTopics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopicTopics_GrammarTopicId_TopicId",
                table: "GrammarTopicTopics",
                columns: new[] { "GrammarTopicId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarTopicTopics_TopicId",
                table: "GrammarTopicTopics",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrammarCommonMistakeTranslations");

            migrationBuilder.DropTable(
                name: "GrammarExampleTranslations");

            migrationBuilder.DropTable(
                name: "GrammarExceptionNoteTranslations");

            migrationBuilder.DropTable(
                name: "GrammarLinkedDialogues");

            migrationBuilder.DropTable(
                name: "GrammarLinkedExercises");

            migrationBuilder.DropTable(
                name: "GrammarLinkedTalkTopics");

            migrationBuilder.DropTable(
                name: "GrammarLinkedWords");

            migrationBuilder.DropTable(
                name: "GrammarPrerequisiteLinks");

            migrationBuilder.DropTable(
                name: "GrammarRelatedTopicLinks");

            migrationBuilder.DropTable(
                name: "GrammarRuleSummaryTranslations");

            migrationBuilder.DropTable(
                name: "GrammarSectionTranslations");

            migrationBuilder.DropTable(
                name: "GrammarTopicTopics");

            migrationBuilder.DropTable(
                name: "GrammarCommonMistakes");

            migrationBuilder.DropTable(
                name: "GrammarExamples");

            migrationBuilder.DropTable(
                name: "GrammarExceptionNotes");

            migrationBuilder.DropTable(
                name: "GrammarRuleSummaries");

            migrationBuilder.DropTable(
                name: "GrammarSections");

            migrationBuilder.DropTable(
                name: "GrammarTopics");
        }
    }
}
