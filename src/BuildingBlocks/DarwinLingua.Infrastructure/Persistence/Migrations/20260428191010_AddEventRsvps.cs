using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRsvps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventRsvps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationEventSlug = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ParticipantName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ParticipantEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRsvps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_ConversationEventSlug_ParticipantEmail",
                table: "EventRsvps",
                columns: new[] { "ConversationEventSlug", "ParticipantEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvps_ConversationEventSlug_Status",
                table: "EventRsvps",
                columns: new[] { "ConversationEventSlug", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRsvps");
        }
    }
}
