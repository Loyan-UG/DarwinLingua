using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

public partial class AddWritingTemplateLocalizedMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "TitleTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "ShortDescriptionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "SituationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "ExplanationTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "TemplateTextTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "SampleFilledVersionTranslationsJson" text NOT NULL DEFAULT '[]';
            ALTER TABLE "WritingTemplates" ADD COLUMN IF NOT EXISTS "LinkedCourseLessonSlugsJson" text NOT NULL DEFAULT '[]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "LinkedCourseLessonSlugsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "SampleFilledVersionTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "TemplateTextTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "ExplanationTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "SituationTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "ShortDescriptionTranslationsJson";
            ALTER TABLE "WritingTemplates" DROP COLUMN IF EXISTS "TitleTranslationsJson";
            """);
    }
}
