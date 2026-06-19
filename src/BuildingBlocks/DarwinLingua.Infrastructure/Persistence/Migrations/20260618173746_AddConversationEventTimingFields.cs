using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationEventTimingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAtUtc",
                table: "ConversationEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartsAtUtc",
                table: "ConversationEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationEvents_StartsAtUtc_PublicationStatus",
                table: "ConversationEvents",
                columns: new[] { "StartsAtUtc", "PublicationStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConversationEvents_StartsAtUtc_PublicationStatus",
                table: "ConversationEvents");

            migrationBuilder.DropColumn(
                name: "EndsAtUtc",
                table: "ConversationEvents");

            migrationBuilder.DropColumn(
                name: "StartsAtUtc",
                table: "ConversationEvents");
        }
    }
}
