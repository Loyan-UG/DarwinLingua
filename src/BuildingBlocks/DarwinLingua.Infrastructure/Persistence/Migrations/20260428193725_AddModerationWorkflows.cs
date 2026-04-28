using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListingReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListingType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ListingKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModerationDecisionAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserReportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DecisionStatus = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DecidedBy = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DecisionNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationDecisionAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizerVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerProfileSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RequestedByEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlockerEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    BlockedEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SourcePartnerRequestId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReporterEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    TargetType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TargetKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ReportedUserEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DecisionNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DecidedBy = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListingReviews_ListingType_ListingKey_Status",
                table: "ListingReviews",
                columns: new[] { "ListingType", "ListingKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationDecisionAudits_UserReportId_CreatedAtUtc",
                table: "ModerationDecisionAudits",
                columns: new[] { "UserReportId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerVerifications_OrganizerProfileSlug_Status",
                table: "OrganizerVerifications",
                columns: new[] { "OrganizerProfileSlug", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockedEmail",
                table: "UserBlocks",
                column: "BlockedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockerEmail_BlockedEmail",
                table: "UserBlocks",
                columns: new[] { "BlockerEmail", "BlockedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_Status_CreatedAtUtc",
                table: "UserReports",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_TargetType_TargetKey",
                table: "UserReports",
                columns: new[] { "TargetType", "TargetKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingReviews");

            migrationBuilder.DropTable(
                name: "ModerationDecisionAudits");

            migrationBuilder.DropTable(
                name: "OrganizerVerifications");

            migrationBuilder.DropTable(
                name: "UserBlocks");

            migrationBuilder.DropTable(
                name: "UserReports");
        }
    }
}
