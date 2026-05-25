using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpressionSensitiveEducationalLanguageMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SensitiveContentKind",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "none");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSensitiveOptIn",
                table: "ExpressionEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresVerifiedAdult",
                table: "ExpressionEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UsagePolicy",
                table: "ExpressionEntries",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "safe-to-use");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SensitiveContentKind",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "RequiresSensitiveOptIn",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "RequiresVerifiedAdult",
                table: "ExpressionEntries");

            migrationBuilder.DropColumn(
                name: "UsagePolicy",
                table: "ExpressionEntries");
        }
    }
}
