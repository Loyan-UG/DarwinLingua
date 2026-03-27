using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWordLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WordLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordLabels_WordEntries_WordEntryId",
                        column: x => x.WordEntryId,
                        principalTable: "WordEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WordLabels_WordEntryId_Kind_Key",
                table: "WordLabels",
                columns: new[] { "WordEntryId", "Kind", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordLabels_WordEntryId_Kind_SortOrder",
                table: "WordLabels",
                columns: new[] { "WordEntryId", "Kind", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordLabels");
        }
    }
}
