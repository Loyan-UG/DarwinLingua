using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetLearningLanguageScopeToConversationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.AddColumn<string>(
                    name: "TargetLearningLanguageCode",
                    table: "ConversationEvents",
                    type: "TEXT",
                    maxLength: 16,
                    nullable: false,
                    defaultValue: "de");

                migrationBuilder.AddColumn<string>(
                    name: "TargetLearningLanguageCode",
                    table: "OrganizerProfileSupportedLevels",
                    type: "TEXT",
                    maxLength: 16,
                    nullable: false,
                    defaultValue: "de");

                migrationBuilder.AddColumn<string>(
                    name: "TargetLearningLanguageCode",
                    table: "EventRsvps",
                    type: "TEXT",
                    maxLength: 16,
                    nullable: false,
                    defaultValue: "de");

                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Slug";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_OrganizerProfileSlug";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_City_IsOnline_PriceType";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Category_PublicationStatus";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_StartsAtUtc_PublicationStatus";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_CefrLevel";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventRsvps_ConversationEventSlug_ParticipantEmail";""");
                migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventRsvps_ConversationEventSlug_Status";""");

                CreateScopedIndexes(migrationBuilder);
                return;
            }

            migrationBuilder.Sql(
                """
                ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';
                ALTER TABLE "OrganizerProfileSupportedLevels" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';
                ALTER TABLE "EventRsvps" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';

                DROP INDEX IF EXISTS "IX_ConversationEvents_Slug";
                DROP INDEX IF EXISTS "IX_ConversationEvents_OrganizerProfileSlug";
                DROP INDEX IF EXISTS "IX_ConversationEvents_City_IsOnline_PriceType";
                DROP INDEX IF EXISTS "IX_ConversationEvents_Category_PublicationStatus";
                DROP INDEX IF EXISTS "IX_ConversationEvents_StartsAtUtc_PublicationStatus";
                DROP INDEX IF EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_CefrLevel";
                DROP INDEX IF EXISTS "IX_EventRsvps_ConversationEventSlug_ParticipantEmail";
                DROP INDEX IF EXISTS "IX_EventRsvps_ConversationEventSlug_Status";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_Category_Publi";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_City_IsOnline_";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_OrganizerProfi";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_Slug";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_StartsAtUtc_Pu";
                DROP INDEX IF EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_TargetLea";
                DROP INDEX IF EXISTS "IX_EventRsvps_TargetLearningLanguageCode_ConversationEventSlug_";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationEvents_Target_Slug"
                    ON "ConversationEvents" ("TargetLearningLanguageCode", "Slug");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_Target_Organizer"
                    ON "ConversationEvents" ("TargetLearningLanguageCode", "OrganizerProfileSlug");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_Target_CityOnlinePrice"
                    ON "ConversationEvents" ("TargetLearningLanguageCode", "City", "IsOnline", "PriceType");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_Target_CategoryStatus"
                    ON "ConversationEvents" ("TargetLearningLanguageCode", "Category", "PublicationStatus");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_Target_StartStatus"
                    ON "ConversationEvents" ("TargetLearningLanguageCode", "StartsAtUtc", "PublicationStatus");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizerProfileLevels_Target_Level"
                    ON "OrganizerProfileSupportedLevels" ("OrganizerProfileId", "TargetLearningLanguageCode", "CefrLevel");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventRsvps_Target_EventEmail"
                    ON "EventRsvps" ("TargetLearningLanguageCode", "ConversationEventSlug", "ParticipantEmail");
                CREATE INDEX IF NOT EXISTS "IX_EventRsvps_Target_EventStatus"
                    ON "EventRsvps" ("TargetLearningLanguageCode", "ConversationEventSlug", "Status");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                DropScopedIndexes(migrationBuilder);

                migrationBuilder.DropColumn(name: "TargetLearningLanguageCode", table: "ConversationEvents");
                migrationBuilder.DropColumn(name: "TargetLearningLanguageCode", table: "OrganizerProfileSupportedLevels");
                migrationBuilder.DropColumn(name: "TargetLearningLanguageCode", table: "EventRsvps");

                CreateLegacyIndexes(migrationBuilder);
                return;
            }

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_ConversationEvents_Target_Slug";
                DROP INDEX IF EXISTS "IX_ConversationEvents_Target_Organizer";
                DROP INDEX IF EXISTS "IX_ConversationEvents_Target_CityOnlinePrice";
                DROP INDEX IF EXISTS "IX_ConversationEvents_Target_CategoryStatus";
                DROP INDEX IF EXISTS "IX_ConversationEvents_Target_StartStatus";
                DROP INDEX IF EXISTS "IX_OrganizerProfileLevels_Target_Level";
                DROP INDEX IF EXISTS "IX_EventRsvps_Target_EventEmail";
                DROP INDEX IF EXISTS "IX_EventRsvps_Target_EventStatus";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_Category_Publi";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_City_IsOnline_";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_OrganizerProfi";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_Slug";
                DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_StartsAtUtc_Pu";
                DROP INDEX IF EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_TargetLea";
                DROP INDEX IF EXISTS "IX_EventRsvps_TargetLearningLanguageCode_ConversationEventSlug_";

                ALTER TABLE "ConversationEvents" DROP COLUMN IF EXISTS "TargetLearningLanguageCode";
                ALTER TABLE "OrganizerProfileSupportedLevels" DROP COLUMN IF EXISTS "TargetLearningLanguageCode";
                ALTER TABLE "EventRsvps" DROP COLUMN IF EXISTS "TargetLearningLanguageCode";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationEvents_Slug" ON "ConversationEvents" ("Slug");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_OrganizerProfileSlug" ON "ConversationEvents" ("OrganizerProfileSlug");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_City_IsOnline_PriceType" ON "ConversationEvents" ("City", "IsOnline", "PriceType");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_Category_PublicationStatus" ON "ConversationEvents" ("Category", "PublicationStatus");
                CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_StartsAtUtc_PublicationStatus" ON "ConversationEvents" ("StartsAtUtc", "PublicationStatus");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_CefrLevel" ON "OrganizerProfileSupportedLevels" ("OrganizerProfileId", "CefrLevel");
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventRsvps_ConversationEventSlug_ParticipantEmail" ON "EventRsvps" ("ConversationEventSlug", "ParticipantEmail");
                CREATE INDEX IF NOT EXISTS "IX_EventRsvps_ConversationEventSlug_Status" ON "EventRsvps" ("ConversationEventSlug", "Status");
                """);
        }

        private static void CreateScopedIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Target_Slug",
                table: "ConversationEvents",
                columns: ["TargetLearningLanguageCode", "Slug"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Target_Organizer",
                table: "ConversationEvents",
                columns: ["TargetLearningLanguageCode", "OrganizerProfileSlug"]);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Target_CityOnlinePrice",
                table: "ConversationEvents",
                columns: ["TargetLearningLanguageCode", "City", "IsOnline", "PriceType"]);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Target_CategoryStatus",
                table: "ConversationEvents",
                columns: ["TargetLearningLanguageCode", "Category", "PublicationStatus"]);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_Target_StartStatus",
                table: "ConversationEvents",
                columns: ["TargetLearningLanguageCode", "StartsAtUtc", "PublicationStatus"]);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerProfileLevels_Target_Level",
                table: "OrganizerProfileSupportedLevels",
                columns: ["OrganizerProfileId", "TargetLearningLanguageCode", "CefrLevel"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_Target_EventEmail",
                table: "EventRsvps",
                columns: ["TargetLearningLanguageCode", "ConversationEventSlug", "ParticipantEmail"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_Target_EventStatus",
                table: "EventRsvps",
                columns: ["TargetLearningLanguageCode", "ConversationEventSlug", "Status"]);
        }

        private static void DropScopedIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Target_Slug";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Target_Organizer";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Target_CityOnlinePrice";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Target_CategoryStatus";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_Target_StartStatus";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_OrganizerProfileLevels_Target_Level";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventRsvps_Target_EventEmail";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventRsvps_Target_EventStatus";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_Category_Publi";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_City_IsOnline_";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_OrganizerProfi";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_Slug";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ConversationEvents_TargetLearningLanguageCode_StartsAtUtc_Pu";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_TargetLea";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_EventRsvps_TargetLearningLanguageCode_ConversationEventSlug_";""");
        }

        private static void CreateLegacyIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(name: "IX_ConversationEvents_Slug", table: "ConversationEvents", column: "Slug", unique: true);
            migrationBuilder.CreateIndex(name: "IX_ConversationEvents_OrganizerProfileSlug", table: "ConversationEvents", column: "OrganizerProfileSlug");
            migrationBuilder.CreateIndex(name: "IX_ConversationEvents_City_IsOnline_PriceType", table: "ConversationEvents", columns: ["City", "IsOnline", "PriceType"]);
            migrationBuilder.CreateIndex(name: "IX_ConversationEvents_Category_PublicationStatus", table: "ConversationEvents", columns: ["Category", "PublicationStatus"]);
            migrationBuilder.CreateIndex(name: "IX_ConversationEvents_StartsAtUtc_PublicationStatus", table: "ConversationEvents", columns: ["StartsAtUtc", "PublicationStatus"]);
            migrationBuilder.CreateIndex(name: "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_CefrLevel", table: "OrganizerProfileSupportedLevels", columns: ["OrganizerProfileId", "CefrLevel"], unique: true);
            migrationBuilder.CreateIndex(name: "IX_EventRsvps_ConversationEventSlug_ParticipantEmail", table: "EventRsvps", columns: ["ConversationEventSlug", "ParticipantEmail"], unique: true);
            migrationBuilder.CreateIndex(name: "IX_EventRsvps_ConversationEventSlug_Status", table: "EventRsvps", columns: ["ConversationEventSlug", "Status"]);
        }
    }
}
