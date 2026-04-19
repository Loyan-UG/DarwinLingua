using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWordLexicalForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WordLexicalForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartOfSpeech = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Article = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    PluralForm = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    InfinitiveForm = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordLexicalForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordLexicalForms_WordEntries_WordEntryId",
                        column: x => x.WordEntryId,
                        principalTable: "WordEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WordLexicalForms_PrimaryPerWordEntry",
                table: "WordLexicalForms",
                column: "WordEntryId",
                unique: true,
                filter: "\"IsPrimary\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WordLexicalForms_WordEntryId_PartOfSpeech",
                table: "WordLexicalForms",
                columns: new[] { "WordEntryId", "PartOfSpeech" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordLexicalForms_WordEntryId_SortOrder",
                table: "WordLexicalForms",
                columns: new[] { "WordEntryId", "SortOrder" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "WordLexicalForms" (
                    "Id",
                    "WordEntryId",
                    "PartOfSpeech",
                    "Article",
                    "PluralForm",
                    "InfinitiveForm",
                    "SortOrder",
                    "IsPrimary",
                    "CreatedAtUtc",
                    "UpdatedAtUtc")
                SELECT
                    "Id",
                    "Id",
                    "PartOfSpeech",
                    "Article",
                    "PluralForm",
                    "InfinitiveForm",
                    1,
                    1,
                    "CreatedAtUtc",
                    "UpdatedAtUtc"
                FROM "WordEntries"
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "WordLexicalForms"
                    WHERE "WordLexicalForms"."WordEntryId" = "WordEntries"."Id");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordLexicalForms");
        }
    }
}
