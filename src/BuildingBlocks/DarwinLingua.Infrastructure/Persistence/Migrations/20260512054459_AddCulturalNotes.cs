using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCulturalNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CulturalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CefrLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 96, nullable: false),
                    Context = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    SectionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    ExamplesJson = table.Column<string>(type: "TEXT", nullable: false),
                    DoNotesJson = table.Column<string>(type: "TEXT", nullable: false),
                    DontNotesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SensitivityWarning = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LinkedDialogueSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedExpressionSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedWritingTemplateSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedTalkTopicSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedCourseLessonSlugsJson = table.Column<string>(type: "TEXT", nullable: false),
                    PublicationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CulturalNotes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CulturalNotes_CefrLevel_Category",
                table: "CulturalNotes",
                columns: new[] { "CefrLevel", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_CulturalNotes_Slug",
                table: "CulturalNotes",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CulturalNotes");
        }
    }
}
