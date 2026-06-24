using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetLearningLanguageScopeToConversationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.AddColumn<string>(
                    name: "TargetLearningLanguageCode",
                    table: "ConversationStarterPacks",
                    type: "TEXT",
                    maxLength: 16,
                    nullable: false,
                    defaultValue: "de");

                migrationBuilder.AddColumn<string>(
                    name: "TargetLearningLanguageCode",
                    table: "EventPreparationPacks",
                    type: "TEXT",
                    maxLength: 16,
                    nullable: false,
                    defaultValue: "de");

                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationStarterPacks_Slug";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationStarterPacks_CefrLevel_Situation_Tone_ConversationGoal";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventPreparationPacks_Slug";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventPreparationPacks_CefrLevel_Category_EventType";""");

                migrationBuilder.CreateIndex(
                    name: "IX_ConversationStarterPacks_TargetLearningLanguageCode_Slug",
                    table: "ConversationStarterPacks",
                    columns: ["TargetLearningLanguageCode", "Slug"],
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_ConversationStarterPacks_TargetLearningLanguageCode_CefrLevel_Situation_Tone_ConversationGoal",
                    table: "ConversationStarterPacks",
                    columns: ["TargetLearningLanguageCode", "CefrLevel", "Situation", "Tone", "ConversationGoal"]);

                migrationBuilder.CreateIndex(
                    name: "IX_EventPreparationPacks_TargetLearningLanguageCode_Slug",
                    table: "EventPreparationPacks",
                    columns: ["TargetLearningLanguageCode", "Slug"],
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_EventPreparationPacks_TargetLearningLanguageCode_CefrLevel_Category_EventType",
                    table: "EventPreparationPacks",
                    columns: ["TargetLearningLanguageCode", "CefrLevel", "Category", "EventType"]);

                return;
            }

            migrationBuilder.Sql(
                """
                ALTER TABLE "ConversationStarterPacks" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';
                ALTER TABLE "EventPreparationPacks" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';

                DROP INDEX IF EXISTS "IX_ConversationStarterPacks_Slug";
                DROP INDEX IF EXISTS "IX_ConversationStarterPacks_CefrLevel_Situation_Tone_ConversationGoal";
                DROP INDEX IF EXISTS "IX_EventPreparationPacks_Slug";
                DROP INDEX IF EXISTS "IX_EventPreparationPacks_CefrLevel_Category_EventType";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPacks_TargetLearningLanguageCode_Slug"
                    ON "ConversationStarterPacks" ("TargetLearningLanguageCode", "Slug");
                CREATE INDEX IF NOT EXISTS "IX_ConversationStarterPacks_TargetLearningLanguageCode_CefrLevel_Situation_Tone_ConversationGoal"
                    ON "ConversationStarterPacks" ("TargetLearningLanguageCode", "CefrLevel", "Situation", "Tone", "ConversationGoal");

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationPacks_TargetLearningLanguageCode_Slug"
                    ON "EventPreparationPacks" ("TargetLearningLanguageCode", "Slug");
                CREATE INDEX IF NOT EXISTS "IX_EventPreparationPacks_TargetLearningLanguageCode_CefrLevel_Category_EventType"
                    ON "EventPreparationPacks" ("TargetLearningLanguageCode", "CefrLevel", "Category", "EventType");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationStarterPacks_TargetLearningLanguageCode_Slug";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationStarterPacks_TargetLearningLanguageCode_CefrLevel_Situation_Tone_ConversationGoal";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventPreparationPacks_TargetLearningLanguageCode_Slug";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventPreparationPacks_TargetLearningLanguageCode_CefrLevel_Category_EventType";""");

                migrationBuilder.DropColumn(
                    name: "TargetLearningLanguageCode",
                    table: "ConversationStarterPacks");

                migrationBuilder.DropColumn(
                    name: "TargetLearningLanguageCode",
                    table: "EventPreparationPacks");

                migrationBuilder.CreateIndex(
                    name: "IX_ConversationStarterPacks_Slug",
                    table: "ConversationStarterPacks",
                    column: "Slug",
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_ConversationStarterPacks_CefrLevel_Situation_Tone_ConversationGoal",
                    table: "ConversationStarterPacks",
                    columns: ["CefrLevel", "Situation", "Tone", "ConversationGoal"]);

                migrationBuilder.CreateIndex(
                    name: "IX_EventPreparationPacks_Slug",
                    table: "EventPreparationPacks",
                    column: "Slug",
                    unique: true);

                migrationBuilder.CreateIndex(
                    name: "IX_EventPreparationPacks_CefrLevel_Category_EventType",
                    table: "EventPreparationPacks",
                    columns: ["CefrLevel", "Category", "EventType"]);

                return;
            }

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_ConversationStarterPacks_TargetLearningLanguageCode_Slug";
                DROP INDEX IF EXISTS "IX_ConversationStarterPacks_TargetLearningLanguageCode_CefrLevel_Situation_Tone_ConversationGoal";
                DROP INDEX IF EXISTS "IX_EventPreparationPacks_TargetLearningLanguageCode_Slug";
                DROP INDEX IF EXISTS "IX_EventPreparationPacks_TargetLearningLanguageCode_CefrLevel_Category_EventType";

                ALTER TABLE "ConversationStarterPacks" DROP COLUMN IF EXISTS "TargetLearningLanguageCode";
                ALTER TABLE "EventPreparationPacks" DROP COLUMN IF EXISTS "TargetLearningLanguageCode";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPacks_Slug"
                    ON "ConversationStarterPacks" ("Slug");
                CREATE INDEX IF NOT EXISTS "IX_ConversationStarterPacks_CefrLevel_Situation_Tone_ConversationGoal"
                    ON "ConversationStarterPacks" ("CefrLevel", "Situation", "Tone", "ConversationGoal");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationPacks_Slug"
                    ON "EventPreparationPacks" ("Slug");
                CREATE INDEX IF NOT EXISTS "IX_EventPreparationPacks_CefrLevel_Category_EventType"
                    ON "EventPreparationPacks" ("CefrLevel", "Category", "EventType");
                """);
        }
    }
}
