using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleplayScenarioLocalizedMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionTranslationsJson",
                table: "RoleplayScenarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "LearnerGoalTranslationsJson",
                table: "RoleplayScenarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TitleTranslationsJson",
                table: "RoleplayScenarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionTranslationsJson",
                table: "RoleplayScenarios");

            migrationBuilder.DropColumn(
                name: "LearnerGoalTranslationsJson",
                table: "RoleplayScenarios");

            migrationBuilder.DropColumn(
                name: "TitleTranslationsJson",
                table: "RoleplayScenarios");
        }
    }
}
