using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpressionEligibilityMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdultContentCategory",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeaningTransparency",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumAge",
                table: "ExpressionEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAdultAccess",
                table: "ExpressionEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SafetyRating",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "general");

            migrationBuilder.AddColumn<string>(
                name: "TeachingReason",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdultContentCategory",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "MeaningTransparency",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "MinimumAge",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "RequiresAdultAccess",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "SafetyRating",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "TeachingReason",
                table: "ExpressionEntries");
        }
    }
}
