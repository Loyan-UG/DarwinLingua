using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpressionEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpressionEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExpressionText = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    LiteralMeaningText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    ActualMeaningText = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    UsageExplanation = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    ExpressionType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Register = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IsRisky = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionExamples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    GermanText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionExamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionExamples_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionLinkedExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionLinkedExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionLinkedExercises_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionLinkedWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    WordSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionLinkedWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionLinkedWords_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionMeanings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    ActualMeaningText = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    LiteralMeaningText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    UsageExplanation = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionMeanings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionMeanings_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionTopics_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpressionTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WarningType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionWarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionWarnings_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatedExpressionLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpressionEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedExpressionLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatedExpressionLinks_ExpressionEntries_ExpressionEntryId",
                        column: x => x.ExpressionEntryId,
                        principalTable: "ExpressionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionExampleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionExampleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionExampleTranslations_ExpressionExamples_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "ExpressionExamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpressionWarningTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpressionWarningTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpressionWarningTranslations_ExpressionWarnings_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "ExpressionWarnings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionEntries_CefrLevel_ExpressionType_Register_Category",
                table: "ExpressionEntries",
                columns: new[] { "CefrLevel", "ExpressionType", "Register", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionEntries_Slug",
                table: "ExpressionEntries",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionExamples_ExpressionEntryId_SortOrder",
                table: "ExpressionExamples",
                columns: new[] { "ExpressionEntryId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionExampleTranslations_OwnerId_LanguageCode",
                table: "ExpressionExampleTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionLinkedExercises_ExpressionEntryId_SortOrder",
                table: "ExpressionLinkedExercises",
                columns: new[] { "ExpressionEntryId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionLinkedExercises_ExpressionEntryId_TargetSlug",
                table: "ExpressionLinkedExercises",
                columns: new[] { "ExpressionEntryId", "TargetSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionLinkedWords_ExpressionEntryId_SortOrder",
                table: "ExpressionLinkedWords",
                columns: new[] { "ExpressionEntryId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionLinkedWords_WordSlug",
                table: "ExpressionLinkedWords",
                column: "WordSlug");

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionMeanings_ExpressionEntryId_LanguageCode",
                table: "ExpressionMeanings",
                columns: new[] { "ExpressionEntryId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionTopics_ExpressionEntryId_TopicId",
                table: "ExpressionTopics",
                columns: new[] { "ExpressionEntryId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionTopics_TopicId",
                table: "ExpressionTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionWarnings_ExpressionEntryId_WarningType",
                table: "ExpressionWarnings",
                columns: new[] { "ExpressionEntryId", "WarningType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpressionWarningTranslations_OwnerId_LanguageCode",
                table: "ExpressionWarningTranslations",
                columns: new[] { "OwnerId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedExpressionLinks_ExpressionEntryId_SortOrder",
                table: "RelatedExpressionLinks",
                columns: new[] { "ExpressionEntryId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedExpressionLinks_ExpressionEntryId_TargetSlug",
                table: "RelatedExpressionLinks",
                columns: new[] { "ExpressionEntryId", "TargetSlug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpressionExampleTranslations");

            migrationBuilder.DropTable(
                name: "ExpressionLinkedExercises");

            migrationBuilder.DropTable(
                name: "ExpressionLinkedWords");

            migrationBuilder.DropTable(
                name: "ExpressionMeanings");

            migrationBuilder.DropTable(
                name: "ExpressionTopics");

            migrationBuilder.DropTable(
                name: "ExpressionWarningTranslations");

            migrationBuilder.DropTable(
                name: "RelatedExpressionLinks");

            migrationBuilder.DropTable(
                name: "ExpressionExamples");

            migrationBuilder.DropTable(
                name: "ExpressionWarnings");

            migrationBuilder.DropTable(
                name: "ExpressionEntries");
        }
    }
}
