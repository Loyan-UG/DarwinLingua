using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WritingTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    Situation = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Register = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TemplateText = table.Column<string>(type: "TEXT", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    VariablesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SampleFilledVersion = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedGrammarTopicSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedWordSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExpressionSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExerciseSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WritingTemplates_CefrLevel_Category_Register",
                table: "WritingTemplates",
                columns: new[] { "CefrLevel", "Category", "Register" });

            migrationBuilder.CreateIndex(
                name: "IX_WritingTemplates_Slug",
                table: "WritingTemplates",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WritingTemplates");
        }
    }
}
