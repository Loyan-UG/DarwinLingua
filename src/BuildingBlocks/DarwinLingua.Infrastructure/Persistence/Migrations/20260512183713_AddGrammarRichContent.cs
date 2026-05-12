using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGrammarRichContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentRevision",
                table: "GrammarTopics",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSlotsJson",
                table: "GrammarTopics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionLocalizedJson",
                table: "GrammarTopics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleLocalizedJson",
                table: "GrammarTopics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalizedBlocksJson",
                table: "GrammarSections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionKey",
                table: "GrammarSections",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentRevision",
                table: "GrammarTopics");

            migrationBuilder.DropColumn(
                name: "ImageSlotsJson",
                table: "GrammarTopics");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionLocalizedJson",
                table: "GrammarTopics");

            migrationBuilder.DropColumn(
                name: "TitleLocalizedJson",
                table: "GrammarTopics");

            migrationBuilder.DropColumn(
                name: "LocalizedBlocksJson",
                table: "GrammarSections");

            migrationBuilder.DropColumn(
                name: "SectionKey",
                table: "GrammarSections");
        }
    }
}
