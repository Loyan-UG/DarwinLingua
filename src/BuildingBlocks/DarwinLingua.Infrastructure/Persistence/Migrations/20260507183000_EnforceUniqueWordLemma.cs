using DarwinLingua.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DarwinLinguaDbContext))]
    [Migration("20260507183000_EnforceUniqueWordLemma")]
    public partial class EnforceUniqueWordLemma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_WordEntries_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_WordEntries_Search_NormalizedLemma";""");
            migrationBuilder.Sql("""CREATE UNIQUE INDEX IF NOT EXISTS "UX_WordEntries_NormalizedLemma" ON "WordEntries" ("NormalizedLemma");""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "UX_WordEntries_NormalizedLemma";""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_WordEntries_Search_NormalizedLemma" ON "WordEntries" ("NormalizedLemma");""");
            migrationBuilder.Sql("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordEntries_NormalizedLemma_PartOfSpeech_PrimaryCefrLevel" ON "WordEntries" ("NormalizedLemma", "PartOfSpeech", "PrimaryCefrLevel");""");
        }
    }
}
