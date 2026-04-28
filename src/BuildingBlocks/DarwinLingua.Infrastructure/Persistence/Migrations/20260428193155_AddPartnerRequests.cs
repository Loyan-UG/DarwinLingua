using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequesterEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    TargetLearnerProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpenerTemplateKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RespondedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRequests_RequesterEmail_CreatedAtUtc",
                table: "PartnerRequests",
                columns: new[] { "RequesterEmail", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRequests_RequesterEmail_TargetLearnerProfileId_Status",
                table: "PartnerRequests",
                columns: new[] { "RequesterEmail", "TargetLearnerProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRequests_TargetLearnerProfileId_Status",
                table: "PartnerRequests",
                columns: new[] { "TargetLearnerProfileId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerRequests");
        }
    }
}
