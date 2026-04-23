using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWordCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WordLexicalForms_PrimaryPerWordEntry",
                table: "WordLexicalForms");

            migrationBuilder.CreateTable(
                name: "WordCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordCollectionEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordCollectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordCollectionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordCollectionEntries_WordCollections_WordCollectionId",
                        column: x => x.WordCollectionId,
                        principalTable: "WordCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WordCollectionEntries_WordEntries_WordEntryId",
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
                filter: "\"IsPrimary\"");

            migrationBuilder.CreateIndex(
                name: "IX_WordCollectionEntries_WordCollectionId_SortOrder",
                table: "WordCollectionEntries",
                columns: new[] { "WordCollectionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordCollectionEntries_WordCollectionId_WordEntryId",
                table: "WordCollectionEntries",
                columns: new[] { "WordCollectionId", "WordEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordCollectionEntries_WordEntryId",
                table: "WordCollectionEntries",
                column: "WordEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_WordCollections_Slug",
                table: "WordCollections",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordCollectionEntries");

            migrationBuilder.DropTable(
                name: "WordCollections");

            migrationBuilder.DropIndex(
                name: "IX_WordLexicalForms_PrimaryPerWordEntry",
                table: "WordLexicalForms");

            migrationBuilder.CreateIndex(
                name: "IX_WordLexicalForms_PrimaryPerWordEntry",
                table: "WordLexicalForms",
                column: "WordEntryId",
                unique: true,
                filter: "\"IsPrimary\" = 1");
        }
    }
}
