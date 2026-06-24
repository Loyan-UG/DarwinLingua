using System;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddUserTargetLearningLanguage : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetLearningLanguageCode",
                table: "UserLearningProfiles",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: ContentLanguageRequirements.DefaultTargetLearningLanguageCode);

            return;
        }

        migrationBuilder.Sql(
            $$"""
            ALTER TABLE "UserLearningProfiles"
            ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT '{{ContentLanguageRequirements.DefaultTargetLearningLanguageCode}}';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.DropColumn(
                name: "TargetLearningLanguageCode",
                table: "UserLearningProfiles");

            return;
        }

        migrationBuilder.Sql(
            """
            ALTER TABLE "UserLearningProfiles"
            DROP COLUMN IF EXISTS "TargetLearningLanguageCode";
            """);
    }
}
