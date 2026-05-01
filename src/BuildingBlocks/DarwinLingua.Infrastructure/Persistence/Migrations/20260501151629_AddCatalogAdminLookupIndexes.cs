using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAdminLookupIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WordLabels_Kind_Key_WordEntryId",
                table: "WordLabels",
                columns: new[] { "Kind", "Key", "WordEntryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WordLabels_Kind_Key_WordEntryId",
                table: "WordLabels");
        }
    }
}
