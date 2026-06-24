using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DarwinLinguaDbContext))]
    [Migration("20260624124000_AddCountryContextScopeToCountryGuidanceNotes")]
    public partial class AddCountryContextScopeToCountryGuidanceNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.AddColumn<string>(
                    name: "CountryContextCode",
                    table: "CountryGuidanceNotes",
                    type: "TEXT",
                    maxLength: 8,
                    nullable: false,
                    defaultValue: "DE");

                migrationBuilder.Sql(
                    """
                    DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_CefrLevel_Category";
                    DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_Slug";
                    CREATE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetCountryFilters"
                        ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "CountryContextCode", "CefrLevel", "Category");
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetCountrySlug"
                        ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "CountryContextCode", "Slug");
                    """);
                return;
            }

            migrationBuilder.Sql(
                """
                ALTER TABLE "CountryGuidanceNotes" ADD COLUMN IF NOT EXISTS "CountryContextCode" character varying(8) NOT NULL DEFAULT 'DE';
                DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_CefrLevel_Category";
                DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_Slug";
                CREATE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetCountryFilters"
                    ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "CountryContextCode", "CefrLevel", "Category");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetCountrySlug"
                    ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "CountryContextCode", "Slug");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.Sql(
                    """
                    DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetCountryFilters";
                    DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetCountrySlug";
                    """);

                migrationBuilder.DropColumn(
                    name: "CountryContextCode",
                    table: "CountryGuidanceNotes");

                migrationBuilder.Sql(
                    """
                    CREATE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_CefrLevel_Category"
                        ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "CefrLevel", "Category");
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_Slug"
                        ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "Slug");
                    """);
                return;
            }

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetCountryFilters";
                DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_TargetCountrySlug";
                ALTER TABLE "CountryGuidanceNotes" DROP COLUMN IF EXISTS "CountryContextCode";
                CREATE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_CefrLevel_Category"
                    ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "CefrLevel", "Category");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_TargetLearningLanguageCode_Slug"
                    ON "CountryGuidanceNotes" ("TargetLearningLanguageCode", "Slug");
                """);
        }
    }
}
