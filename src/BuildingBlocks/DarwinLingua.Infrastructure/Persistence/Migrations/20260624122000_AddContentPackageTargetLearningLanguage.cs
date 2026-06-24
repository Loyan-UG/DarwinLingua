using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPackageTargetLearningLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Browse_Cefr_NormalizedLemma",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Search_ActiveNormalizedLemma",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Search_NormalizedLemma",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_ContentPackages_PackageId",
                table: "ContentPackages");

            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "ContentPackages",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "de");

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Browse_Language_Cefr_NormalizedLemma",
                table: "WordEntries",
                columns: new[] { "LanguageCode", "PrimaryCefrLevel", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Language_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel",
                table: "WordEntries",
                columns: new[] { "LanguageCode", "NormalizedLemma", "PartOfSpeech", "PrimaryCefrLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Search_Language_ActiveNormalizedLemma",
                table: "WordEntries",
                columns: new[] { "LanguageCode", "PublicationStatus", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Search_Language_NormalizedLemma",
                table: "WordEntries",
                columns: new[] { "LanguageCode", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPackages_TargetLearningLanguageCode_PackageId",
                table: "ContentPackages",
                columns: new[] { "TargetLearningLanguageCode", "PackageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Browse_Language_Cefr_NormalizedLemma",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Language_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Search_Language_ActiveNormalizedLemma",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Search_Language_NormalizedLemma",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_ContentPackages_TargetLearningLanguageCode_PackageId",
                table: "ContentPackages");

            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "ContentPackages");

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Browse_Cefr_NormalizedLemma",
                table: "WordEntries",
                columns: new[] { "PrimaryCefrLevel", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel",
                table: "WordEntries",
                columns: new[] { "NormalizedLemma", "PartOfSpeech", "PrimaryCefrLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Search_ActiveNormalizedLemma",
                table: "WordEntries",
                columns: new[] { "PublicationStatus", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Search_NormalizedLemma",
                table: "WordEntries",
                column: "NormalizedLemma");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPackages_PackageId",
                table: "ContentPackages",
                column: "PackageId",
                unique: true);
        }
    }
}
