using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations;

public partial class AddCourseLessonActivityBlocks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "CourseLessons" ADD COLUMN IF NOT EXISTS "ActivityBlocksJson" text NOT NULL DEFAULT '[]';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "CourseLessons" DROP COLUMN IF EXISTS "ActivityBlocksJson";
            """);
    }
}
