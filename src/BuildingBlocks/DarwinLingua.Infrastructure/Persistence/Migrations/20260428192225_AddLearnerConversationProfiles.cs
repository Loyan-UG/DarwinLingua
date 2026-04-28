using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnerConversationProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearnerConversationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CityRegion = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    InteractionPreference = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    GermanLevel = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    HelperLanguageCodes = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ConversationGoals = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AvailabilityNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Visibility = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    HasConfirmedAdult = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerConversationProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerConversationProfiles_OwnerEmail",
                table: "LearnerConversationProfiles",
                column: "OwnerEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerConversationProfiles_Visibility_CityRegion_GermanLevel",
                table: "LearnerConversationProfiles",
                columns: new[] { "Visibility", "CityRegion", "GermanLevel" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerConversationProfiles");
        }
    }
}
