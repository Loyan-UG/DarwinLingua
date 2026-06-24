using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DarwinLinguaDbContext))]
    [Migration("20260614000000_RenameCulturalNotesToCountryGuidanceNotes")]
    public partial class RenameCulturalNotesToCountryGuidanceNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.Sql(
                    """
                    ALTER TABLE "CulturalNotes" RENAME TO "CountryGuidanceNotes";
                    DROP INDEX IF EXISTS "IX_CulturalNotes_CefrLevel_Category";
                    DROP INDEX IF EXISTS "IX_CulturalNotes_Slug";
                    CREATE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_CefrLevel_Category"
                        ON "CountryGuidanceNotes" ("CefrLevel", "Category");
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_CountryGuidanceNotes_Slug"
                        ON "CountryGuidanceNotes" ("Slug");
                    UPDATE "UserContentProgress"
                    SET "ContentOwnerType" = 'country-guidance'
                    WHERE "ContentOwnerType" = 'cultural-note';
                    """);
                return;
            }

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('"CountryGuidanceNotes"') IS NULL AND to_regclass('"CulturalNotes"') IS NOT NULL THEN
                        ALTER TABLE "CulturalNotes" RENAME TO "CountryGuidanceNotes";
                    END IF;

                    IF to_regclass('"CountryGuidanceNotes"') IS NOT NULL THEN
                        IF EXISTS (
                            SELECT 1 FROM pg_constraint
                            WHERE conname = 'PK_CulturalNotes'
                              AND conrelid = '"CountryGuidanceNotes"'::regclass
                        ) THEN
                            ALTER TABLE "CountryGuidanceNotes" RENAME CONSTRAINT "PK_CulturalNotes" TO "PK_CountryGuidanceNotes";
                        END IF;
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_CefrLevel_Category"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_CefrLevel_Category" RENAME TO "IX_CountryGuidanceNotes_CefrLevel_Category";
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_Slug"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_Slug" RENAME TO "IX_CountryGuidanceNotes_Slug";
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_Title_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_Title_Trgm" RENAME TO "IX_CountryGuidanceNotes_Title_Trgm";
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_Description_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_Description_Trgm" RENAME TO "IX_CountryGuidanceNotes_Description_Trgm";
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_Context_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_Context_Trgm" RENAME TO "IX_CountryGuidanceNotes_Context_Trgm";
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_Slug_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_Slug_Trgm" RENAME TO "IX_CountryGuidanceNotes_Slug_Trgm";
                    END IF;

                    IF to_regclass('"IX_CulturalNotes_SearchFilters"') IS NOT NULL THEN
                        ALTER INDEX "IX_CulturalNotes_SearchFilters" RENAME TO "IX_CountryGuidanceNotes_SearchFilters";
                    END IF;
                END $$;

                UPDATE "UserContentProgress"
                SET "ContentOwnerType" = 'country-guidance'
                WHERE "ContentOwnerType" = 'cultural-note';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.Sql(
                    """
                    ALTER TABLE "CountryGuidanceNotes" RENAME TO "CulturalNotes";
                    DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_CefrLevel_Category";
                    DROP INDEX IF EXISTS "IX_CountryGuidanceNotes_Slug";
                    CREATE INDEX IF NOT EXISTS "IX_CulturalNotes_CefrLevel_Category"
                        ON "CulturalNotes" ("CefrLevel", "Category");
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_CulturalNotes_Slug"
                        ON "CulturalNotes" ("Slug");
                    UPDATE "UserContentProgress"
                    SET "ContentOwnerType" = 'cultural-note'
                    WHERE "ContentOwnerType" = 'country-guidance';
                    """);
                return;
            }

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('"CulturalNotes"') IS NULL AND to_regclass('"CountryGuidanceNotes"') IS NOT NULL THEN
                        ALTER TABLE "CountryGuidanceNotes" RENAME TO "CulturalNotes";
                    END IF;

                    IF to_regclass('"CulturalNotes"') IS NOT NULL THEN
                        IF EXISTS (
                            SELECT 1 FROM pg_constraint
                            WHERE conname = 'PK_CountryGuidanceNotes'
                              AND conrelid = '"CulturalNotes"'::regclass
                        ) THEN
                            ALTER TABLE "CulturalNotes" RENAME CONSTRAINT "PK_CountryGuidanceNotes" TO "PK_CulturalNotes";
                        END IF;
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_CefrLevel_Category"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_CefrLevel_Category" RENAME TO "IX_CulturalNotes_CefrLevel_Category";
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_Slug"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_Slug" RENAME TO "IX_CulturalNotes_Slug";
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_Title_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_Title_Trgm" RENAME TO "IX_CulturalNotes_Title_Trgm";
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_Description_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_Description_Trgm" RENAME TO "IX_CulturalNotes_Description_Trgm";
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_Context_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_Context_Trgm" RENAME TO "IX_CulturalNotes_Context_Trgm";
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_Slug_Trgm"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_Slug_Trgm" RENAME TO "IX_CulturalNotes_Slug_Trgm";
                    END IF;

                    IF to_regclass('"IX_CountryGuidanceNotes_SearchFilters"') IS NOT NULL THEN
                        ALTER INDEX "IX_CountryGuidanceNotes_SearchFilters" RENAME TO "IX_CulturalNotes_SearchFilters";
                    END IF;
                END $$;

                UPDATE "UserContentProgress"
                SET "ContentOwnerType" = 'cultural-note'
                WHERE "ContentOwnerType" = 'country-guidance';
                """);
        }
    }
}
