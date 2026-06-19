using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

public partial class AddCourseLessonActivityBlocks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivityBlocksJson",
                table: "CourseLessons",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            return;
        }

        migrationBuilder.Sql(
            """
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "ActivityBlocksJson" text NOT NULL DEFAULT '[]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.DropColumn(name: "ActivityBlocksJson", table: "CourseLessons");

            return;
        }

        migrationBuilder.Sql(
            """
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "ActivityBlocksJson";
            """);
    }
}
