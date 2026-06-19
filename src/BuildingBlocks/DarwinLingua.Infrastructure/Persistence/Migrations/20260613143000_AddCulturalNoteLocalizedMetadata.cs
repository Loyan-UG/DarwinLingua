using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

public partial class AddCulturalNoteLocalizedMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ContextTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SectionsTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ExamplesTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DoNotesTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DontNotesTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SensitivityWarningTranslationsJson",
                table: "CulturalNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            return;
        }

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
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.DropColumn(name: "SensitivityWarningTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "DontNotesTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "DoNotesTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "ExamplesTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "SectionsTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "ContextTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "ShortDescriptionTranslationsJson", table: "CulturalNotes");
            migrationBuilder.DropColumn(name: "TitleTranslationsJson", table: "CulturalNotes");

            return;
        }

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
