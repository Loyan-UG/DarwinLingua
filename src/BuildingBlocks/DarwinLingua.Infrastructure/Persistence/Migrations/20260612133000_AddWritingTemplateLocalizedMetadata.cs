using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

public partial class AddWritingTemplateLocalizedMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleTranslationsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionTranslationsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SituationTranslationsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ExplanationTranslationsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TemplateTextTranslationsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SampleFilledVersionTranslationsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "LinkedCourseLessonSlugsJson",
                table: "WritingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            return;
        }

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
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.DropColumn(name: "LinkedCourseLessonSlugsJson", table: "WritingTemplates");
            migrationBuilder.DropColumn(name: "SampleFilledVersionTranslationsJson", table: "WritingTemplates");
            migrationBuilder.DropColumn(name: "TemplateTextTranslationsJson", table: "WritingTemplates");
            migrationBuilder.DropColumn(name: "ExplanationTranslationsJson", table: "WritingTemplates");
            migrationBuilder.DropColumn(name: "SituationTranslationsJson", table: "WritingTemplates");
            migrationBuilder.DropColumn(name: "ShortDescriptionTranslationsJson", table: "WritingTemplates");
            migrationBuilder.DropColumn(name: "TitleTranslationsJson", table: "WritingTemplates");

            return;
        }

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
