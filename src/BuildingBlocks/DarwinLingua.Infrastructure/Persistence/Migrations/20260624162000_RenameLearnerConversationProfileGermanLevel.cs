using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarwinLingua.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameLearnerConversationProfileGermanLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GermanLevel",
                table: "LearnerConversationProfiles",
                newName: "LearningLevel");

            migrationBuilder.RenameIndex(
                name: "IX_LearnerConversationProfiles_Visibility_CityRegion_GermanLevel",
                table: "LearnerConversationProfiles",
                newName: "IX_LearnerConversationProfiles_Visibility_CityRegion_LearningLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_LearnerConversationProfiles_Visibility_CityRegion_LearningLevel",
                table: "LearnerConversationProfiles",
                newName: "IX_LearnerConversationProfiles_Visibility_CityRegion_GermanLevel");

            migrationBuilder.RenameColumn(
                name: "LearningLevel",
                table: "LearnerConversationProfiles",
                newName: "GermanLevel");
        }
    }
}
