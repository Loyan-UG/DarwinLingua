using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

public partial class AddCulturalNoteLocalizedMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "ShortDescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "ContextTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "SectionsTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "ExamplesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "DoNotesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "DontNotesTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "CulturalNotes" ADD COLUMN IF NOT EXISTS "SensitivityWarningTranslationsJson" text NOT NULL DEFAULT '[]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "SensitivityWarningTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "DontNotesTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "DoNotesTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "ExamplesTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "SectionsTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "ContextTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "ShortDescriptionTranslationsJson";
            ALTER TABLE "CulturalNotes" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            """);
    }
}
